using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ECommerce.Web.Services;
using ECommerce.Web.Models.ViewModels;

namespace ECommerce.Web.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly IApiService _apiService;

        public CartController(IApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            try
            {
                Console.WriteLine($"AddToCart başladı - ProductId: {productId}, Quantity: {quantity}");
                
                var token = HttpContext.Request.Cookies["token"];
                Console.WriteLine($"Token durumu: {(string.IsNullOrEmpty(token) ? "Boş" : $"Var (İlk 20 karakter: {token.Substring(0, 20)}...)")}");
                
                if (string.IsNullOrEmpty(token))
                {
                    foreach(var cookie in Request.Cookies.Keys)
                    {
                        Response.Cookies.Delete(cookie);
                    }
                    return Json(new { success = false, message = "Oturum süreniz dolmuş. Lütfen tekrar giriş yapın." });
                }

                // Tüm cookie'leri logla
                Console.WriteLine("Mevcut cookie'ler:");
                foreach (var cookie in Request.Cookies)
                {
                    Console.WriteLine($"{cookie.Key}: {(cookie.Key == "token" ? $"{cookie.Value.Substring(0, 20)}..." : cookie.Value)}");
                }

                var model = new AddToCartViewModel
                {
                    ProductId = productId,
                    Quantity = quantity
                };

                Console.WriteLine($"API isteği gönderiliyor... Model: {System.Text.Json.JsonSerializer.Serialize(model)}");
                var response = await _apiService.PostAsync<ApiResponse<object>>("api/Cart/AddToCart", model, token);
                Console.WriteLine($"API yanıtı: {System.Text.Json.JsonSerializer.Serialize(response)}");
                
                if (response?.Success == true)
                {
                    return Json(new { success = true, message = "Ürün sepete eklendi." });
                }
                
                return Json(new { success = false, message = response?.Message ?? "Bir hata oluştu." });
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP Hatası: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                
                if (ex.Message.Contains("401"))
                {
                    foreach (var cookie in Request.Cookies.Keys)
                    {
                        Response.Cookies.Delete(cookie);
                    }
                    return Json(new { success = false, message = "Oturum süreniz dolmuş. Lütfen tekrar giriş yapın." });
                }
                return Json(new { success = false, message = $"Ürün sepete eklenirken bir hata oluştu: {ex.Message}" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Genel Hata: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return Json(new { success = false, message = "Ürün sepete eklenirken bir hata oluştu." });
            }
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            try
            {
                var token = HttpContext.Request.Cookies["token"];
                var response = await _apiService.GetAsync<ApiResponse<List<CartViewModel>>>("api/Cart", token);

                if (response?.Success == true)
                {
                    return View(response.Data);
                }

                return View(new List<CartViewModel>());
            }
            catch
            {
                return View(new List<CartViewModel>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            try
            {
                Console.WriteLine($"UpdateQuantity başladı - ProductId: {productId}, Quantity: {quantity}");
                
                var token = HttpContext.Request.Cookies["token"];
                if (string.IsNullOrEmpty(token))
                    return RedirectToAction("Login", "Account");

                var model = new
                {
                    ProductId = productId,
                    Quantity = quantity
                };

                await _apiService.PostAsync<ApiResponse<object>>("api/Cart/UpdateQuantity", model, token);
                TempData["SuccessMessage"] = "Sepet güncellendi.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sepet güncelleme hatası: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = "Sepet güncellenirken bir hata oluştu.";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveItem(int productId)
        {
            try
            {
                var token = HttpContext.Request.Cookies["token"];
                if (string.IsNullOrEmpty(token))
                    return RedirectToAction("Login", "Account");

                var response = await _apiService.DeleteAsync<ApiResponse<object>>($"api/Cart/RemoveItem/{productId}", token);
                
                if (response.Success)
                {
                    TempData["SuccessMessage"] = "Ürün sepetten kaldırıldı.";
                }
                else
                {
                    TempData["ErrorMessage"] = response.Message;
                }
                
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sepetten ürün kaldırma hatası: {ex.Message}");
                TempData["ErrorMessage"] = "Ürün sepetten kaldırılırken bir hata oluştu.";
                return RedirectToAction("Index");
            }
        }
    }
} 