using ECommerce.Entity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        // Identity işlemleri için gerekli servisler
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _roleManager = roleManager;
        }

        // Kullanıcı girişi
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                    return BadRequest(new { message = "Kullanıcı bulunamadı" });

                var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);

                if (result.Succeeded)
                {
                    // Kullanıcının rollerini al
                    var roles = await _userManager.GetRolesAsync(user);

                    var token = CreateToken(user, roles);

                    return Ok(new { 
                        Message = "Giriş başarılı",
                        Token = token,
                        UserId = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Roles = roles
                    });
                }

                return BadRequest(new { message = "Email veya şifre hatalı" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Giriş yapılırken bir hata oluştu: {ex.Message}" });
            }
        }

        // Token oluşturma metodu
        private string CreateToken(ApplicationUser user, IList<string> roles)
        {
            var jwtSecret = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtSecret))
            {
                throw new InvalidOperationException("JWT Secret key is missing!");
            }

            Console.WriteLine($"CreateToken Method - User: {user.Email}, Roles: {string.Join(",", roles)}");
            
            // Temel claims tanımla
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("FirstName", user.FirstName ?? ""),
                new Claim("LastName", user.LastName ?? "")
            };
            
            // Rol claims'lerini ekle - standart formatta
            foreach (var role in roles)
            {
                Console.WriteLine($"Adding role: {role}");
                claims.Add(new Claim(ClaimTypes.Role, role));
                // JWT doğrulaması için özel bir claim daha ekleyelim (çift garantili)
                claims.Add(new Claim("role", role));
            }
            
            Console.WriteLine($"Created {claims.Count} claims for token");
            
            // Token oluştur
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            
            // Token ayarları
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = credentials,
                Issuer = "http://localhost:5169",     // Statik tanımlama - appsettings.json'a göre değişebilir
                Audience = "http://localhost:5156"    // Statik tanımlama - appsettings.json'a göre değişebilir
            };
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var token = tokenHandler.WriteToken(securityToken);
            
            Console.WriteLine($"Token created successfully, length: {token.Length}");
            return token;
        }

        // Yeni kullanıcı kaydı
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            try
            {
                // Aynı email ile kayıtlı kullanıcı var mı kontrol et
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "Bu email adresi zaten kullanılıyor" });
                }

                // Yeni kullanıcı oluştur
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Address = model.Address,
                    CreatedAt = DateTime.Now
                };

                // Kullanıcıyı kaydet
                var result = await _userManager.CreateAsync(user, model.Password);
                
                // Kayıt başarılıysa
                if (result.Succeeded)
                {
                    // Otomatik olarak tüm kullanıcılara Seller rolü ata
                    // Seller rolünü kontrol et, yoksa oluştur
                    if (!await _roleManager.RoleExistsAsync("Seller"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Seller"));
                    }

                    // Kullanıcıya satıcı rolü ata
                    await _userManager.AddToRoleAsync(user, "Seller");

                    Console.WriteLine($"Kullanıcı {user.Email} başarıyla oluşturuldu ve Seller rolü atandı!");

                    return Ok(new { message = "Kayıt başarılı! Giriş yapabilirsiniz." });
                }

                // Kayıt başarısızsa hataları döndür
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Kayıt olurken bir hata oluştu: {ex.Message}" });
            }
        }

        // Çıkış yap
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            // Oturumu kapat
            await _signInManager.SignOutAsync();
            return Ok("Çıkış yapıldı");
        }

        // Admin kullanıcı oluşturma
        [HttpPost("create-admin")]
        public async Task<IActionResult> CreateAdmin([FromBody] RegisterDto model)
        {
            try
            {
                // Email kontrolü
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "Bu email adresi zaten kullanılıyor" });
                }

                // Admin kullanıcısı oluştur
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Address = model.Address,
                    CreatedAt = DateTime.Now
                };

                // Kullanıcıyı kaydet
                var result = await _userManager.CreateAsync(user, model.Password);
                
                if (result.Succeeded)
                {
                    // Admin rolünü kontrol et, yoksa oluştur
                    if (!await _roleManager.RoleExistsAsync("Admin"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Admin"));
                    }

                    // Kullanıcıya admin rolü ata
                    await _userManager.AddToRoleAsync(user, "Admin");

                    return Ok(new { message = "Admin kullanıcısı başarıyla oluşturuldu!" });
                }

                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Admin oluşturulurken bir hata oluştu: {ex.Message}" });
            }
        }

        // Satıcı kullanıcı oluşturma
        [HttpPost("create-seller")]
        public async Task<IActionResult> CreateSeller([FromBody] RegisterDto model)
        {
            try
            {
                // Email kontrolü
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "Bu email adresi zaten kullanılıyor" });
                }

                // Satıcı kullanıcısı oluştur
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Address = model.Address,
                    CreatedAt = DateTime.Now
                };

                // Kullanıcıyı kaydet
                var result = await _userManager.CreateAsync(user, model.Password);
                
                if (result.Succeeded)
                {
                    // Seller rolünü kontrol et, yoksa oluştur
                    if (!await _roleManager.RoleExistsAsync("Seller"))
                    {
                        await _roleManager.CreateAsync(new IdentityRole("Seller"));
                    }

                    // Kullanıcıya satıcı rolü ata
                    await _userManager.AddToRoleAsync(user, "Seller");

                    return Ok(new { message = "Satıcı hesabı başarıyla oluşturuldu! Giriş yapabilirsiniz." });
                }

                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Satıcı hesabı oluşturulurken bir hata oluştu: {ex.Message}" });
            }
        }
    }

    // Login için veri modeli
    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    // Kayıt için veri modeli
    public class RegisterDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
    }
} 