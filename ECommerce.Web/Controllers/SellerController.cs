using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ECommerce.Web.Models;
using ECommerce.Web.Services;
using System.Threading.Tasks;
using ECommerce.Web.Models.ViewModels; // Doğru namespace
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering; // SelectList için gerekli using direktifi
using System.Text.Json;

namespace ECommerce.Web.Controllers
{
    [Authorize] // Sadece giriş yapmış kullanıcılar erişebilir, rol kısıtlaması kaldırıldı
    public class SellerController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<SellerController> _logger;

        public SellerController(IApiService apiService, ILogger<SellerController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                try
                {
                    var token = Request.Cookies["token"];
                    _logger.LogInformation($"Token Durumu: {(string.IsNullOrEmpty(token) ? "YOK" : token.Substring(0, Math.Min(token.Length, 20)))}");
                    
                    List<ECommerce.Web.Models.ViewModels.ProductViewModel> products;
                    
                    try 
                    {
                        _logger.LogInformation("API'ye istek gönderiliyor: api/Product/Seller/Products");
                        products = await _apiService.GetAsync<List<ECommerce.Web.Models.ViewModels.ProductViewModel>>("api/Product/Seller/Products", token);
                        _logger.LogInformation($"API'den {(products == null ? "NULL" : products.Count.ToString())} ürün alındı");
                        
                        if (products != null && products.Any())
                        {
                            foreach (var product in products)
                            {
                                _logger.LogInformation($"Ürün Bilgisi: ID={product.Id}, Ad={product.Name}, Fiyat={product.Price}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "API'den ürün listesi alınırken hata oluştu: {Message}", ex.Message);
                        products = new List<ECommerce.Web.Models.ViewModels.ProductViewModel>();
                    }
                    
                    if (products == null)
                    {
                        products = new List<ECommerce.Web.Models.ViewModels.ProductViewModel>();
                    }
                    
                    return View(products);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ürün listesi alınırken bir hata oluştu");
                    TempData["ErrorMessage"] = "Ürün listesi alınırken bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
                    return View(new List<ECommerce.Web.Models.ViewModels.ProductViewModel>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Index sayfası yüklenirken bir hata oluştu");
                TempData["ErrorMessage"] = "Sayfa yüklenirken bir hata oluştu: " + ex.Message;
                return View(new List<ECommerce.Web.Models.ViewModels.ProductViewModel>());
            }
        }

        public async Task<IActionResult> CreateProduct()
        {
            try
            {
                // Kategorileri API'den çek
                var token = Request.Cookies["token"];
                
                // Kategori listesini al
                var categories = await _apiService.GetAsync<List<ECommerce.Web.Models.ViewModels.CategoryViewModel>>("api/Category", token);
                
                // ViewBag'e kategori listesini ekle
                ViewBag.Categories = categories?.Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                }).ToList() ?? new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
                
                if (categories == null || !categories.Any())
                {
                    _logger.LogWarning("Kategori listesi boş veya yüklenemedi");
                    TempData["WarningMessage"] = "Kategori listesi yüklenemedi. Lütfen sonra tekrar deneyin.";
                }
                
                // Boş bir model oluştur
                var model = new ProductCreateModel 
                {
                    // Varsayılan bir resim URL'si ekle
                    ImageUrl = "https://via.placeholder.com/350" 
                };
                
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateProduct sayfası yüklenirken bir hata oluştu");
                TempData["ErrorMessage"] = "Sayfa yüklenirken bir hata oluştu: " + ex.Message;
                ViewBag.Categories = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
                
                // Hata durumunda da boş model oluştur
                var model = new ProductCreateModel 
                {
                    ImageUrl = "https://via.placeholder.com/350"
                };
                
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(ProductCreateModel model)
        {
            // Null olabilecek listeleri initialize et
            if (model.Features == null)
            {
                model.Features = new List<ProductFeature>();
            }
            
            // Bu alanların validasyon hatalarını temizle
            ModelState.Remove("Features");
            
            if (!ModelState.IsValid)
            {
                try
                {
                    // ModelState geçerli değilse, kategori listesini tekrar yükle
                    var token = Request.Cookies["token"];
                    var categories = await _apiService.GetAsync<List<ECommerce.Web.Models.ViewModels.CategoryViewModel>>("api/Category", token);
                    
                    ViewBag.Categories = categories?.Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name
                    }).ToList() ?? new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Kategoriler yüklenirken hata oluştu");
                    ViewBag.Categories = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
                }
                
                return View(model);
            }

            try
            {
                // Cookie'den token al
                var token = Request.Cookies["token"];
                
                if (string.IsNullOrEmpty(token))
                {
                    TempData["ErrorMessage"] = "Oturumunuz sona ermiş veya yetkiniz bulunmamaktadır. Lütfen tekrar giriş yapın.";
                    return RedirectToAction("Login", "Account");
                }
                
                // API'nin beklediği formata dönüştür
                var apiModel = new
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    Stock = model.StockQuantity, // API'de "Stock" olarak tanımlanmış
                    CategoryId = model.CategoryId,
                    ImageUrl = model.ImageUrl, // Artık URL doğrudan kullanılabilir
                    Features = model.Features ?? new List<ProductFeature>(),
                    AdditionalImageUrls = new List<string>() // Yeni ürün için boş liste
                };
                
                // Gönderilen veriyi logla
                _logger.LogInformation($"API'ye gönderilen JSON: {JsonSerializer.Serialize(apiModel)}");
                
                // Doğru endpoint'i kullan: api/Product (Create metodu yok)
                _logger.LogInformation("API isteği yapılıyor: POST api/Product");
                var result = await _apiService.PostAsync<ApiResponse<object>>("api/Product", apiModel, token);
                
                _logger.LogInformation($"API yanıtı: Success={result.Success}, Message={result.Message}");
                
                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Ürün başarıyla oluşturuldu!";
                    _logger.LogInformation("Ürün başarıyla oluşturuldu, Index sayfasına yönlendiriliyor.");
                    return RedirectToAction(nameof(Index));
                }
                
                _logger.LogWarning($"API ürün oluşturma başarısız: {result.Message}");
                ModelState.AddModelError("", result.Message);
                return View(model);
            }
            catch (UnauthorizedAccessException)
            {
                TempData["ErrorMessage"] = "Bu işlemi gerçekleştirmek için yetkiniz bulunmamaktadır veya oturumunuz sona ermiştir.";
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateProduct işlemi sırasında bir hata oluştu");
                ModelState.AddModelError("", "Beklenmeyen bir hata oluştu: " + ex.Message);
                return View(model);
            }
        }

        public async Task<IActionResult> EditProduct(int id)
        {
            try
            {
                var token = Request.Cookies["token"];
                if (string.IsNullOrEmpty(token))
                {
                    TempData["ErrorMessage"] = "Oturum süresi dolmuş. Lütfen tekrar giriş yapın.";
                    return RedirectToAction("Login", "Account");
                }

                // Kategori listesini API'den alın
                var categoriesResponse = await _apiService.GetAsync<ApiResponse<List<CategoryViewModel>>>("api/Category", token);
                if (!categoriesResponse.Success)
                {
                    TempData["ErrorMessage"] = "Kategoriler yüklenirken bir hata oluştu: " + categoriesResponse.Message;
                    return RedirectToAction(nameof(Index));
                }
                ViewBag.Categories = new SelectList(categoriesResponse.Data, "Id", "Name");

                // Ürün detaylarını API'den alın
                var productResponse = await _apiService.GetAsync<ApiResponse<ProductViewModel>>($"api/Product/{id}", token);
                if (!productResponse.Success || productResponse.Data == null)
                {
                    TempData["ErrorMessage"] = "Ürün bulunamadı veya yüklenirken bir hata oluştu: " + productResponse.Message;
                    return RedirectToAction(nameof(Index));
                }

                return View(productResponse.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün düzenleme sayfası yüklenirken hata oluştu. ProductId: {ProductId}", id);
                TempData["ErrorMessage"] = "Bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(ProductViewModel model)
        {
            try
            {
                // Null olabilecek listeleri initialize et
                if (model.Features == null)
                {
                    model.Features = new List<ProductFeatureViewModel>();
                }
                
                if (model.AdditionalImageUrls == null)
                {
                    model.AdditionalImageUrls = new List<string>();
                }
                
                // Bu alanların validasyon hatalarını temizle
                ModelState.Remove("Features");
                ModelState.Remove("AdditionalImageUrls");
                
                // Kategori listesini API'den alın (form doğrulama hatası durumunda dropdown'u doldurmak için)
                var categoriesResponse = await _apiService.GetAsync<ApiResponse<List<CategoryViewModel>>>("api/Category", null);
                if (!categoriesResponse.Success)
                {
                    TempData["ErrorMessage"] = "Kategoriler yüklenirken bir hata oluştu: " + categoriesResponse.Message;
                }
                ViewBag.Categories = new SelectList(categoriesResponse.Data, "Id", "Name");

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Token al
                var token = Request.Cookies["token"];
                if (string.IsNullOrEmpty(token))
                {
                    TempData["ErrorMessage"] = "Oturum süresi dolmuş. Lütfen tekrar giriş yapın.";
                    return RedirectToAction("Login", "Account");
                }

                // API'nin beklediği formata uygun olarak dönüştür
                var apiModel = new
                {
                    Id = model.Id,
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price,
                    Stock = model.StockQuantity,
                    CategoryId = model.CategoryId,
                    ImageUrl = model.ImageUrl ?? "https://via.placeholder.com/350",
                    Features = model.Features ?? new List<ProductFeatureViewModel>(),
                    AdditionalImageUrls = model.AdditionalImageUrls ?? new List<string>()
                };

                // API isteği gönder
                var apiResponse = await _apiService.PutAsync<ApiResponse<object>>($"api/Product/{model.Id}", apiModel, token);
                
                if (apiResponse.Success)
                {
                    TempData["SuccessMessage"] = "Ürün başarıyla güncellendi.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Ürün güncellenirken bir hata oluştu: " + apiResponse.Message;
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün güncellenirken hata oluştu");
                TempData["ErrorMessage"] = "Bir hata oluştu: " + ex.Message;
                
                // Hata durumunda form tekrar gösterilir
                return View(model);
            }
        }

        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                // Cookie'den token al
                var token = Request.Cookies["token"];
                
                if (string.IsNullOrEmpty(token))
                {
                    TempData["ErrorMessage"] = "Oturumunuz sona ermiş veya yetkiniz bulunmamaktadır. Lütfen tekrar giriş yapın.";
                    return RedirectToAction("Login", "Account");
                }
                
                var result = await _apiService.DeleteAsync<ApiResponse<object>>($"api/Product/{id}", token);
                
                if (result.Success)
                {
                    TempData["SuccessMessage"] = "Ürün başarıyla silindi!";
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message;
                }
                
                return RedirectToAction(nameof(Index));
            }
            catch (UnauthorizedAccessException)
            {
                TempData["ErrorMessage"] = "Bu işlemi gerçekleştirmek için yetkiniz bulunmamaktadır veya oturumunuz sona ermiştir.";
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteProduct işlemi sırasında bir hata oluştu");
                TempData["ErrorMessage"] = "Beklenmeyen bir hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
} 