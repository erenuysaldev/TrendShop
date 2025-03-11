using Microsoft.AspNetCore.Mvc;
using ECommerce.Web.Services;
using ECommerce.Web.Models.ViewModels;
using ECommerce.Web.Models.DTOs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace ECommerce.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IApiService _apiService;

        public AccountController(IApiService apiService)
        {
            _apiService = apiService;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var response = await _apiService.PostAsync<LoginResponseDto>("api/Auth/login", model);
                    
                    if (response != null && !string.IsNullOrEmpty(response.Token))
                    {
                        // Önce eski cookie'leri temizle
                        foreach (var cookie in Request.Cookies.Keys)
                        {
                            Response.Cookies.Delete(cookie);
                        }

                        // Token'ı cookie'ye kaydet
                        HttpContext.Response.Cookies.Append("token", response.Token, new CookieOptions
                        {
                            HttpOnly = false,  // JavaScript'in erişimine izin ver
                            Secure = true,
                            SameSite = SameSiteMode.Lax,
                            Expires = DateTime.UtcNow.AddHours(1) // Token ile aynı süre
                        });

                        // Debug için token'ı konsola yazdıralım
                        Console.WriteLine($"Token kaydedildi (ilk 20 karakter): {response.Token.Substring(0, Math.Min(20, response.Token.Length))}...");
                        Console.WriteLine($"Token: {response.Token}");

                        // Kullanıcı bilgilerini cookie'ye kaydet
                        HttpContext.Response.Cookies.Append("firstName", response.FirstName ?? "", new CookieOptions
                        {
                            HttpOnly = false,
                            Secure = true,
                            SameSite = SameSiteMode.Lax
                        });

                        // Claims oluştur
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, response.Email),
                            new Claim(ClaimTypes.NameIdentifier, response.UserId),
                            new Claim("FirstName", response.FirstName ?? ""),
                            new Claim("LastName", response.LastName ?? "")
                        };

                        if (response.Roles != null)
                        {
                            foreach (var role in response.Roles)
                            {
                                claims.Add(new Claim(ClaimTypes.Role, role));
                            }
                        }

                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var principal = new ClaimsPrincipal(identity);

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            principal,
                            new AuthenticationProperties
                            {
                                IsPersistent = true,
                                ExpiresUtc = DateTime.UtcNow.AddDays(1)
                            });

                        // ReturnUrl kontrolü
                        var returnUrl = Request.Cookies["returnUrl"];
                        if (!string.IsNullOrEmpty(returnUrl))
                        {
                            Response.Cookies.Delete("returnUrl");
                            return Redirect(returnUrl);
                        }

                        return RedirectToAction("Index", "Home");
                    }
                    
                    ModelState.AddModelError("", "Giriş başarısız");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Login Error: {ex}");
                    ModelState.AddModelError("", "Giriş sırasında bir hata oluştu");
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // Authentication cookie'yi sil
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            
            // Tüm cookie'leri sil
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }
            
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var registerData = new
                    {
                        model.Email,
                        model.Password,
                        model.FirstName,
                        model.LastName,
                        model.Address
                    };
                    
                    // Tüm kullanıcıları doğrudan satıcı yap - IsSeller seçeneğini göz ardı et
                    string endpoint = "api/Auth/create-seller";
                    
                    var response = await _apiService.PostAsync<ApiResponseDto>(endpoint, registerData);
                    
                    if (response.Success)
                    {
                        TempData["SuccessMessage"] = "Kayıt başarılı! Lütfen giriş yapın.";
                        return RedirectToAction("Login");
                    }
                    else
                    {
                        ModelState.AddModelError("", response.Message);
                        return View(model);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Kayıt işlemi sırasında bir hata oluştu: " + ex.Message);
                }
            }
            return View(model);
        }
    }
} 