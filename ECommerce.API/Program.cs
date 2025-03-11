using ECommerce.Business.Interfaces;
using ECommerce.Business.Services;
using ECommerce.Data.Context;
using ECommerce.Data.Repositories;
using ECommerce.Entity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.FileProviders;
using ECommerce.Data.SeedData;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Diagnostics;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using ECommerce.API.Filters;
using Microsoft.OpenApi.Models;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// HTTPS port tanımlaması
builder.WebHost.UseUrls("http://localhost:5169", "https://localhost:7169");

// DbContext'i ekle
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity servislerini ekle
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
});

// Repository'leri ekle
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<CategoryRepository>();
builder.Services.AddScoped<CartRepository>();
builder.Services.AddScoped<OrderRepository>();

// Business Service'leri ekle
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// API Service'leri ekle
builder.Services.AddScoped<ECommerce.API.Services.Interfaces.IProductService, ECommerce.API.Services.ProductService>();

// Filtreler
builder.Services.AddScoped<AdminAuthorizationFilter>();

// JWT Authentication ayarları
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT Secret key is missing in configuration!");
}

Console.WriteLine($"API Starting - JWT Key Found: {jwtKey.Substring(0, Math.Min(5, jwtKey.Length))}***");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    // Debug için çok önemli - token doğrulama hatalarını detaylı görme
    options.IncludeErrorDetails = true;
    
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Basit ve net doğrulama - sadece en önemli kontrollerle
        ValidateIssuer = false,          // Şimdilik issuer doğrulamayı devre dışı bırakacağız
        ValidateAudience = false,        // Şimdilik audience doğrulamayı devre dışı bırakacağız
        ValidateLifetime = true,         // Token süresi kontrolü aktif
        ValidateIssuerSigningKey = true, // İmza kontrol et (en kritik kısım)
        ClockSkew = TimeSpan.FromMinutes(5), // 5 dakikalık bir tolerans
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        // Rol claim'inin ismini belirt - hem standart hem özel olanı tanımasını sağla
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.NameIdentifier
    };

    // Hata ayıklama ve logging
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("⛔ Authentication failed: " + context.Exception.GetType().Name);
            Console.WriteLine("Error details: " + context.Exception.Message);
            Console.WriteLine("Stack trace: " + context.Exception.StackTrace);
            
            if (context.Exception is SecurityTokenExpiredException)
            {
                Console.WriteLine("Token expired!");
            }
            return Task.CompletedTask;
        },
        
        OnTokenValidated = context =>
        {
            Console.WriteLine("✅ Token validated successfully!");
            
            var claims = context.Principal?.Claims.ToList();
            if (claims != null && claims.Any())
            {
                Console.WriteLine($"Claims count: {claims.Count}");
                foreach (var claim in claims)
                {
                    Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
                }
                
                // Admin ve Seller rollerini özel olarak kontrol et
                var isAdmin = claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
                var isSeller = claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Seller");
                Console.WriteLine($"Is Admin: {isAdmin}, Is Seller: {isSeller}");
            }
            else
            {
                Console.WriteLine("⚠️ No claims found in the token!");
            }
            
            return Task.CompletedTask;
        },
        
        OnChallenge = context =>
        {
            Console.WriteLine("⚠️ Challenge triggered: " + context.Error);
            Console.WriteLine("Error Description: " + context.ErrorDescription);
            
            // Authorization header'ı kontrol et
            if (context.Request.Headers.ContainsKey("Authorization"))
            {
                var authHeader = context.Request.Headers["Authorization"].ToString();
                Console.WriteLine($"Authorization header found: {authHeader.Substring(0, Math.Min(30, authHeader.Length))}...");
            }
            else
            {
                Console.WriteLine("No Authorization header found in the request!");
            }
            
            return Task.CompletedTask;
        },
        
        OnMessageReceived = context =>
        {
            if (context.Request.Headers.ContainsKey("Authorization"))
            {
                var authHeader = context.Request.Headers["Authorization"].ToString();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    var token = authHeader.Substring("Bearer ".Length).Trim();
                    context.Token = token;
                    Console.WriteLine($"Token extracted from header: {token.Substring(0, Math.Min(20, token.Length))}...");
                }
                else
                {
                    Console.WriteLine("Authorization header does not start with 'Bearer '");
                }
            }
            
            Console.WriteLine($"Received token: {(string.IsNullOrEmpty(context.Token) ? "NULL/EMPTY" : context.Token.Substring(0, Math.Min(10, context.Token.Length)) + "...")}");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
});

// Controller'ları ekle
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.MaxDepth = 64; // Varsayılan 32'yi artırıyoruz
});

// Swagger'ı ekle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "E-Commerce API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder
            .WithOrigins("http://localhost:5156") // Angular uygulamasının adresi
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

var app = builder.Build();

// Geliştirme ortamında Swagger'ı kullan
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    // Geliştirme ortamında detaylı hata sayfaları
    app.UseDeveloperExceptionPage();
}

// Global exception handler
app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        
        var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
        if (contextFeature != null)
        {
            Console.WriteLine($"Global hata: {contextFeature.Error}");
            
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                context.Response.StatusCode,
                Message = "Internal Server Error.",
                Detail = app.Environment.IsDevelopment() ? contextFeature.Error.ToString() : "İç sunucu hatası"
            }));
        }
    });
});

// CORS Middleware
app.UseCors("AllowSpecificOrigin");

app.UseHttpsRedirection();

// API sağlık kontrolü endpoint'i
app.MapGet("/health", () => 
{
    Console.WriteLine("Health check - OK");
    return Results.Ok(new { Status = "Healthy", Timestamp = DateTime.UtcNow });
}).AllowAnonymous();

// API durumu kontrolü endpoint'i
app.MapGet("/api/status", () => 
{
    Console.WriteLine("API Status - OK");
    var status = new
    {
        Status = "Running",
        Timestamp = DateTime.UtcNow,
        ApiVersion = "1.0",
        Environment = app.Environment.EnvironmentName,
        HostName = Environment.MachineName
    };
    return Results.Ok(status);
}).AllowAnonymous();

// Middleware sıralaması önemli!
app.UseRouting();

// Bu sıralama önemli - önce Authentication sonra Authorization
app.UseAuthentication();
app.UseAuthorization();

// Statik dosyalar
app.UseStaticFiles();

// Statik dosyalar için klasör yapılandırması
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
    RequestPath = ""
});

// API kontrolü için endpoint (yetkilendirme testi)
app.MapGet("/api/auth-test", (ClaimsPrincipal user) =>
{
    if (user.Identity?.IsAuthenticated == true)
    {
        var claims = user.Claims.Select(c => new { c.Type, c.Value }).ToList();
        var roles = user.Claims
            .Where(c => c.Type == ClaimTypes.Role || c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
            .Select(c => c.Value)
            .ToList();
            
        return Results.Ok(new { 
            IsAuthenticated = true, 
            Username = user.Identity.Name,
            Claims = claims,
            Roles = roles,
            IsAdmin = roles.Contains("Admin")
        });
    }
    return Results.Unauthorized();
}).RequireAuthorization();

// Seed data için servis scope oluştur
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        await AdminSeedData.SeedAdminUser(userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Admin kullanıcısı oluşturulurken bir hata oluştu.");
    }
}

app.MapControllers();

app.Run();
