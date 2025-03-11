using Microsoft.AspNetCore.Mvc;
using ECommerce.Web.Services;
using ECommerce.Web.Models.ViewModels;

namespace ECommerce.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IApiService _apiService;
        private readonly IConfiguration _configuration;

        public ProductController(IApiService apiService, IConfiguration configuration)
        {
            _apiService = apiService;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                ViewBag.ApiUrl = _configuration["ApiSettings:BaseUrl"];
                var response = await _apiService.GetAsync<ApiResponse<List<ProductViewModel>>>("api/Product");
                
                if (response?.Success == true)
                {
                    return View(response.Data);
                }

                TempData["ErrorMessage"] = "Ürünler yüklenirken bir hata oluştu.";
                return View(new List<ProductViewModel>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading products: {ex.Message}");
                TempData["ErrorMessage"] = "Ürünler yüklenirken bir hata oluştu.";
                return View(new List<ProductViewModel>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                ViewBag.ApiUrl = _configuration["ApiSettings:BaseUrl"];
                var response = await _apiService.GetAsync<ApiResponse<ProductViewModel>>($"api/Product/{id}");
                
                if (response?.Success == true)
                {
                    return View(response.Data);
                }

                return NotFound();
            }
            catch
            {
                return NotFound();
            }
        }
    }
} 