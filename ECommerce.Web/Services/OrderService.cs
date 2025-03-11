using ECommerce.Web.Models.ViewModels;
using System.Text.Json;

namespace ECommerce.Web.Services
{
    public class OrderService : IOrderService
    {
        private readonly IApiService _apiService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<OrderService> _logger;

        public OrderService(IApiService apiService, IHttpContextAccessor httpContextAccessor, ILogger<OrderService> logger)
        {
            _apiService = apiService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<int> CreateOrderAsync(OrderCreateViewModel model)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.Request.Cookies["token"];
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token bulunamadı, sipariş oluşturulamadı");
                    return 0;
                }

                _logger.LogInformation("Sipariş oluşturma isteği gönderiliyor: {@OrderInfo}", 
                    new { model.FullName, model.Email, model.PaymentMethod });

                try
                {
                    // Önce ApiResponse<object> olarak almayı dene
                    var apiResponse = await _apiService.PostAsync<ApiResponse<object>>("api/Order/Create", model, token);
                    
                    if (apiResponse != null && apiResponse.Success)
                    {
                        _logger.LogInformation("Sipariş başarıyla oluşturuldu");
                        
                        // Data içinde id var mı kontrol et
                        if (apiResponse.Data != null)
                        {
                            if (apiResponse.Data is JsonElement jsonElement)
                            {
                                if (jsonElement.TryGetProperty("id", out JsonElement idElement) && 
                                    idElement.TryGetInt32(out int orderId))
                                {
                                    _logger.LogInformation("Sipariş ID alındı: {OrderId}", orderId);
                                    return orderId;
                                }
                            }
                        }
                        
                        return 1; // ID alınamadı ama sipariş başarılı
                    }
                    
                    _logger.LogWarning("Sipariş oluşturulamadı: {Message}", apiResponse?.Message ?? "Bilinmeyen hata");
                    return 0;
                }
                catch (Exception)
                {
                    // Alternatif olarak doğrudan object döndürmeyi dene
                    var response = await _apiService.PostAsync<object>("api/Order/Create", model, token);
                    
                    if (response != null)
                    {
                        // API'den dönen yanıtı işle
                        if (response is JsonElement jsonElement)
                        {
                            // Success kontrolü
                            if (jsonElement.TryGetProperty("success", out var successElement) && 
                                successElement.GetBoolean())
                            {
                                // Data içinde id'yi kontrol et
                                if (jsonElement.TryGetProperty("data", out var dataElement))
                                {
                                    if (dataElement.TryGetProperty("id", out JsonElement idElement) && 
                                        idElement.TryGetInt32(out int orderId))
                                    {
                                        _logger.LogInformation("Sipariş başarıyla oluşturuldu. Sipariş ID: {OrderId}", orderId);
                                        return orderId;
                                    }
                                }
                                
                                _logger.LogInformation("Sipariş oluşturuldu ancak ID alınamadı");
                                return 1; // ID alınamadı ama sipariş oluşturuldu
                            }
                        }
                    }

                    _logger.LogWarning("Sipariş oluşturulamadı, API yanıt vermedi veya hatalı yanıt döndü");
                    return 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş oluşturulurken hata oluştu");
                return 0;
            }
        }

        public async Task<List<OrderViewModel>> GetOrdersAsync()
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.Request.Cookies["token"];
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token bulunamadı, siparişler getirilemedi");
                    return new List<OrderViewModel>();
                }

                var orders = await _apiService.GetAsync<List<OrderViewModel>>("api/Order/GetUserOrders", token);
                return orders ?? new List<OrderViewModel>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Siparişler getirilirken hata oluştu");
                return new List<OrderViewModel>();
            }
        }

        public async Task<OrderDetailViewModel> GetOrderDetailAsync(int id)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.Request.Cookies["token"];
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token bulunamadı, sipariş detayı getirilemedi");
                    return null;
                }

                var orderDetail = await _apiService.GetAsync<OrderDetailViewModel>($"api/Order/GetOrderDetail/{id}", token);
                return orderDetail;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş detayı getirilirken hata oluştu. Sipariş ID: {OrderId}", id);
                return null;
            }
        }
    }
} 