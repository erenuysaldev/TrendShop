using ECommerce.Web.Models.ViewModels;

namespace ECommerce.Web.Services
{
    public class CartService : ICartService
    {
        private readonly IApiService _apiService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<CartService> _logger;

        public CartService(IApiService apiService, IHttpContextAccessor httpContextAccessor, ILogger<CartService> logger)
        {
            _apiService = apiService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<List<CartViewModel>> GetCartItemsAsync()
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.Request.Cookies["token"];
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token bulunamadı, sepet öğeleri getirilemedi");
                    return new List<CartViewModel>();
                }

                try
                {
                    // Önce doğrudan ApiResponse<List<CartViewModel>> olarak deneyelim
                    var response = await _apiService.GetAsync<ApiResponse<List<CartViewModel>>>("api/Cart", token);
                    return response?.Data ?? new List<CartViewModel>();
                }
                catch
                {
                    // Eğer yukarıdaki başarısız olursa, doğrudan List<CartViewModel> deneyelim
                    var cartItems = await _apiService.GetAsync<List<CartViewModel>>("api/Cart", token);
                    return cartItems ?? new List<CartViewModel>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sepet öğeleri getirilirken hata oluştu");
                return new List<CartViewModel>();
            }
        }

        public async Task<bool> AddToCartAsync(int productId, int quantity)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.Request.Cookies["token"];
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token bulunamadı, ürün sepete eklenemedi");
                    return false;
                }

                var model = new { ProductId = productId, Quantity = quantity };
                await _apiService.PostAsync<object>("api/Cart/Add", model, token);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün sepete eklenirken hata oluştu. Ürün ID: {ProductId}, Miktar: {Quantity}", productId, quantity);
                return false;
            }
        }

        public async Task<bool> UpdateCartItemAsync(int productId, int quantity)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.Request.Cookies["token"];
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token bulunamadı, sepet öğesi güncellenemedi");
                    return false;
                }

                var model = new { ProductId = productId, Quantity = quantity };
                await _apiService.PutAsync<object>("api/Cart/Update", model, token);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sepet öğesi güncellenirken hata oluştu. Ürün ID: {ProductId}, Miktar: {Quantity}", productId, quantity);
                return false;
            }
        }

        public async Task<bool> RemoveFromCartAsync(int productId)
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.Request.Cookies["token"];
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token bulunamadı, ürün sepetten çıkarılamadı");
                    return false;
                }

                await _apiService.DeleteAsync<object>($"api/Cart/Remove/{productId}", token);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün sepetten çıkarılırken hata oluştu. Ürün ID: {ProductId}", productId);
                return false;
            }
        }

        public async Task<bool> ClearCartAsync()
        {
            try
            {
                var token = _httpContextAccessor.HttpContext?.Request.Cookies["token"];
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Token bulunamadı, sepet temizlenemedi");
                    return false;
                }

                await _apiService.DeleteAsync<object>("api/Cart/Clear", token);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sepet temizlenirken hata oluştu");
                return false;
            }
        }
    }
} 