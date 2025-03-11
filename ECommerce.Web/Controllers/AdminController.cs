using Microsoft.AspNetCore.Mvc;
using ECommerce.Web.Services;
using ECommerce.Web.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System.Net.Http;

namespace ECommerce.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IApiService _apiService;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public AdminController(IApiService apiService, IConfiguration configuration)
        {
            _apiService = apiService;
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        public IActionResult Index()
        {
            // Admin dashboard view'ını döndür
            return View();
        }

        public async Task<IActionResult> Products()
        {
            try
            {
                var token = HttpContext.Request.Cookies["token"];
                ViewBag.ApiUrl = _configuration["ApiSettings:BaseUrl"];
                
                try
                {
                    var categories = await _apiService.GetAsync<List<CategoryViewModel>>("api/Category", token);
                    ViewBag.Categories = categories ?? new List<CategoryViewModel>();
                    Console.WriteLine($"Categories loaded: {categories?.Count ?? 0}");
                }
                catch (Exception categoryEx)
                {
                    Console.WriteLine($"Error loading categories: {categoryEx.Message}");
                    ViewBag.Categories = new List<CategoryViewModel>();
                }

                try
                {
                    var response = await _apiService.GetAsync<ApiResponse<List<ProductViewModel>>>("api/Product", token);
                    Console.WriteLine($"API Response: {response?.Success}, {response?.Message}");
                    Console.WriteLine($"Products loaded: {response?.Data?.Count ?? 0}");
                    return View(response?.Data ?? new List<ProductViewModel>());
                }
                catch (Exception productEx)
                {
                    Console.WriteLine($"Error loading products: {productEx.Message}");
                    return View(new List<ProductViewModel>());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General error: {ex.Message}");
                TempData["ErrorMessage"] = "Veriler yüklenirken bir hata oluştu.";
                ViewBag.Categories = new List<CategoryViewModel>();
                return View(new List<ProductViewModel>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct([FromForm] ProductCreateViewModel model)
        {
            try
            {
                // Model doğrulaması
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                // Form verilerini debug için logla
                Console.WriteLine($"Name: {model.Name}");
                Console.WriteLine($"Description: {model.Description ?? "Boş"}");
                Console.WriteLine($"Price: {model.Price}");
                Console.WriteLine($"Stock: {model.Stock}");
                Console.WriteLine($"CategoryId: {model.CategoryId}");
                Console.WriteLine($"Image: {(model.Image != null ? model.Image.FileName : "Boş")}");
                
                // Minimum değer kontrolü
                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    return Json(new { success = false, message = "Ürün adı zorunludur" });
                }
                
                if (model.Price <= 0)
                {
                    return Json(new { success = false, message = "Fiyat 0'dan büyük olmalıdır" });
                }
                
                if (model.CategoryId <= 0)
                {
                    return Json(new { success = false, message = "Geçerli bir kategori seçilmelidir" });
                }
                
                if (model.Stock < 0)
                {
                    return Json(new { success = false, message = "Stok miktarı negatif olamaz" });
                }

                var token = HttpContext.Request.Cookies["token"];
                
                // Form verilerini hazırla
                var productData = new
                {
                    Name = model.Name.Trim(),
                    Description = model.Description?.Trim() ?? "",
                    Price = model.Price,
                    Stock = model.Stock,
                    CategoryId = model.CategoryId,
                };
                
                Console.WriteLine($"API'ye gönderilen veri: {System.Text.Json.JsonSerializer.Serialize(productData)}");
                
                var response = await _apiService.PostAsync<ApiResponse<object>>("api/Product", productData, token);
                
                if (response?.Success == true)
                {
                    // Ürün eklendi, şimdi resim varsa yükleyebiliriz
                    if (model.Image != null && model.Image.Length > 0)
                    {
                        // TODO: Resim yükleme işlemi ayrı bir endpoint'e yapılabilir
                    }
                    
                    return Json(new { success = true, message = "Ürün başarıyla eklendi." });
                }
                else
                {
                    return Json(new { success = false, message = response?.Message ?? "Ürün eklenirken bir hata oluştu." });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Product Add Error: {ex}");
                return Json(new { success = false, message = "Ürün eklenirken bir hata oluştu: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var token = HttpContext.Request.Cookies["token"];
                await _apiService.DeleteAsync<object>($"api/Product/{id}", token);
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        public async Task<IActionResult> Categories()
        {
            try
            {
                var token = HttpContext.Request.Cookies["token"];
                var categories = await _apiService.GetAsync<List<CategoryViewModel>>("api/Category", token);
                return View(categories);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Kategoriler yüklenirken bir hata oluştu.";
                return View(new List<CategoryViewModel>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(CategoryViewModel model)
        {
            try
            {
                var token = HttpContext.Request.Cookies["token"];
                await _apiService.PostAsync<object>("api/Category", model, token);
                TempData["SuccessMessage"] = "Kategori başarıyla eklendi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Kategori eklenirken bir hata oluştu: " + ex.Message;
                Console.WriteLine($"Category Add Error: {ex}");
            }
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCategory(CategoryViewModel model)
        {
            try
            {
                var token = HttpContext.Request.Cookies["token"];
                await _apiService.PutAsync<object>($"api/Category/{model.Id}", model, token);
                TempData["SuccessMessage"] = "Kategori başarıyla güncellendi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Kategori güncellenirken bir hata oluştu: " + ex.Message;
                Console.WriteLine($"Category Update Error: {ex}");
            }
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var token = HttpContext.Request.Cookies["token"];
                await _apiService.DeleteAsync<object>($"api/Category/{id}", token);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Category Delete Error: {ex}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("GetProduct/{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            try
            {
                Console.WriteLine($"GetProduct çağrıldı: id={id}");
                var token = HttpContext.Request.Cookies["token"];
                
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("Token bulunamadı");
                    return Json(new { success = false, message = "Oturum bilgisi bulunamadı" });
                }
                
                Console.WriteLine("API'ye istek yapılıyor...");
                var result = await _apiService.GetAsync<ProductViewModel>($"api/Product/{id}", token);
                
                if (result == null)
                {
                    Console.WriteLine("Ürün bulunamadı");
                    return Json(new { success = false, message = "Ürün bulunamadı" });
                }
                
                Console.WriteLine($"Ürün bulundu: {result.Name}");
                return Json(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetProduct Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Ürün bilgileri alınırken bir hata oluştu: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProduct([FromBody] ProductViewModel model)
        {
            try
            {
                // Null olabilecek koleksiyonları initialize et
                if (model.Features == null)
                {
                    model.Features = new List<ProductFeatureViewModel>();
                }
                
                if (model.AdditionalImageUrls == null)
                {
                    model.AdditionalImageUrls = new List<string>();
                }
                
                // Bu alanlar için ModelState hatalarını kaldır
                if (ModelState.ContainsKey("Features"))
                {
                    ModelState.Remove("Features");
                }
                
                if (ModelState.ContainsKey("AdditionalImageUrls"))
                {
                    ModelState.Remove("AdditionalImageUrls");
                }
                
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return Json(new { success = false, message = string.Join(", ", errors) });
                }

                // Form verilerini debug için logla
                Console.WriteLine($"Update - Id: {model.Id}");
                Console.WriteLine($"Update - Name: {model.Name}");
                Console.WriteLine($"Update - Description: {model.Description ?? "Boş"}");
                Console.WriteLine($"Update - Price: {model.Price}");
                Console.WriteLine($"Update - Stock: {model.Stock}");
                Console.WriteLine($"Update - CategoryId: {model.CategoryId}");
                Console.WriteLine($"Update - ImageUrl: {model.ImageUrl ?? "Boş"}");
                
                // Minimum değer kontrolü
                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    return Json(new { success = false, message = "Ürün adı zorunludur" });
                }
                
                if (model.Price <= 0)
                {
                    return Json(new { success = false, message = "Fiyat 0'dan büyük olmalıdır" });
                }
                
                if (model.CategoryId <= 0)
                {
                    return Json(new { success = false, message = "Geçerli bir kategori seçilmelidir" });
                }
                
                if (model.Stock < 0)
                {
                    return Json(new { success = false, message = "Stok miktarı negatif olamaz" });
                }

                var token = HttpContext.Request.Cookies["token"];

                // Mevcut ürün bilgilerini al (özellikle resim URL'si için)
                ProductViewModel existingProduct = null;
                try
                {
                    existingProduct = await _apiService.GetAsync<ProductViewModel>($"api/Product/{model.Id}", token);
                }
                catch
                {
                    // Mevcut ürün bulunamazsa devam et 
                }
                
                // Eğer resim URL'si girilmediyse ve mevcut ürünün resim URL'si varsa, onu kullan
                if (string.IsNullOrWhiteSpace(model.ImageUrl) && existingProduct != null && !string.IsNullOrWhiteSpace(existingProduct.ImageUrl))
                {
                    model.ImageUrl = existingProduct.ImageUrl;
                }
                
                // CategoryName boş ise ve CategoryId varsa, kategori adını dolduralım
                if (string.IsNullOrWhiteSpace(model.CategoryName) && model.CategoryId > 0)
                {
                    try 
                    {
                        // Kategoriler ViewBag'den alınabilir
                        var categories = ViewBag.Categories as List<CategoryViewModel>;
                        if (categories != null)
                        {
                            var category = categories.FirstOrDefault(c => c.Id == model.CategoryId);
                            if (category != null)
                            {
                                model.CategoryName = category.Name;
                            }
                        }
                        
                        // Eğer ViewBag'den kategori bulunamazsa, API'den almayı deneyelim
                        if (string.IsNullOrEmpty(model.CategoryName) && existingProduct != null)
                        {
                            model.CategoryName = existingProduct.CategoryName;
                        }
                        
                        // Hala boşsa, "Kategori" yazalım
                        if (string.IsNullOrEmpty(model.CategoryName))
                        {
                            model.CategoryName = "Kategori";
                        }
                    }
                    catch 
                    {
                        // Hata olursa varsayılan bir değer ata
                        model.CategoryName = "Kategori";
                    }
                }
                
                // API'ye güncelleme isteği gönder
                Console.WriteLine($"API'ye gönderilen veri: {System.Text.Json.JsonSerializer.Serialize(model)}");
                
                var response = await _apiService.PutAsync<ApiResponse<object>>($"api/Product", model, token);
                
                if (response?.Success == true)
                {
                    return Json(new { success = true, message = "Ürün başarıyla güncellendi." });
                }
                else
                {
                    return Json(new { success = false, message = response?.Message ?? "Ürün güncellenirken bir hata oluştu." });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Product Update Error: {ex}");
                return Json(new { success = false, message = "Ürün güncellenirken bir hata oluştu: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProductDetails(int id)
        {
            try
            {
                Console.WriteLine($"GetProductDetails çağrıldı: id={id}");
                var token = HttpContext.Request.Cookies["token"];
                
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("Token bulunamadı");
                    return Json(new { success = false, message = "Oturum bilgisi bulunamadı" });
                }
                
                Console.WriteLine("API'ye istek yapılıyor...");
                var result = await _apiService.GetAsync<ProductViewModel>($"api/Product/{id}", token);
                
                if (result == null)
                {
                    Console.WriteLine("Ürün bulunamadı");
                    return Json(new { success = false, message = "Ürün bulunamadı" });
                }
                
                Console.WriteLine($"Ürün bulundu: {result.Name}");
                return Json(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetProductDetails Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Ürün bilgileri alınırken bir hata oluştu: {ex.Message}" });
            }
        }

        public async Task<IActionResult> Orders()
        {
            // Bu özellik geçici olarak devre dışı bırakıldı
            TempData["ErrorMessage"] = "Sipariş yönetimi özelliği geçici olarak devre dışı bırakılmıştır.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            // Bu özellik geçici olarak devre dışı bırakıldı
            TempData["ErrorMessage"] = "Sipariş durumu güncelleme özelliği geçici olarak devre dışı bırakılmıştır.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult TestToken()
        {
            try
            {
                var token = HttpContext.Request.Cookies["token"];
                
                if (string.IsNullOrEmpty(token))
                {
                    return Json(new { success = false, message = "Token bulunamadı" });
                }
                
                // Token'ı decode et ve bilgileri göster
                var tokenParts = token.Split('.');
                if (tokenParts.Length != 3)
                {
                    return Json(new { success = false, message = "Token geçerli bir JWT formatında değil" });
                }
                
                // Base64 decode
                var payload = tokenParts[1];
                // Padding ekle
                payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
                var payloadBytes = Convert.FromBase64String(payload);
                var payloadJson = System.Text.Encoding.UTF8.GetString(payloadBytes);
                
                return Json(new { success = true, token = token, payload = payloadJson });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestApiConnection()
        {
            try
            {
                var token = HttpContext.Request.Cookies["token"];
                
                if (string.IsNullOrEmpty(token))
                {
                    return Json(new { success = false, message = "Token bulunamadı" });
                }
                
                // API'nin temel sağlık kontrolü
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"];
                var tokenInfo = new {
                    TokenLength = token.Length,
                    TokenStart = token.Substring(0, Math.Min(20, token.Length)),
                    TokenEnd = token.Substring(Math.Max(0, token.Length - 20)),
                    ApiBaseUrl = apiBaseUrl
                };
                
                // Test API endpoint'ini çağır
                try {
                    var response = await _httpClient.GetAsync($"{apiBaseUrl}/api/Test/Ping");
                    var pingResult = await response.Content.ReadAsStringAsync();
                    
                    return Json(new { 
                        success = true, 
                        message = "API bağlantı testi", 
                        tokenInfo = tokenInfo,
                        pingStatus = response.StatusCode.ToString(),
                        pingResult = pingResult
                    });
                }
                catch (Exception apiEx) {
                    return Json(new { 
                        success = false, 
                        message = "API bağlantı hatası", 
                        tokenInfo = tokenInfo,
                        error = apiEx.Message
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        [HttpGet]
        public async Task<IActionResult> TestOrdersEndpoint()
        {
            try
            {
                var token = HttpContext.Request.Cookies["token"];
                
                if (string.IsNullOrEmpty(token))
                {
                    return Json(new { success = false, message = "Token bulunamadı" });
                }
                
                var apiBaseUrl = _configuration["ApiSettings:BaseUrl"];
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                
                try {
                    var response = await _httpClient.GetAsync($"{apiBaseUrl}/api/Order/Admin/GetAllOrders");
                    var responseContent = await response.Content.ReadAsStringAsync();
                    
                    return Json(new { 
                        success = response.IsSuccessStatusCode, 
                        statusCode = response.StatusCode.ToString(),
                        content = responseContent,
                        headers = response.Headers.Select(h => new { h.Key, Value = string.Join(", ", h.Value) }).ToList()
                    });
                }
                catch (Exception apiEx) {
                    return Json(new { 
                        success = false, 
                        message = "API orders endpoint hatası", 
                        error = apiEx.Message,
                        stackTrace = apiEx.StackTrace
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        // Diğer action'lar eklenecek...
    }

    public class DashboardViewModel
    {
        public decimal TotalSales { get; set; }
        public int TotalOrders { get; set; }
        public int TotalProducts { get; set; }
        public int TotalUsers { get; set; }
        public List<OrderViewModel> RecentOrders { get; set; }
    }
} 