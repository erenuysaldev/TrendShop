using ECommerce.Web.Models.ViewModels;

namespace ECommerce.Web.Services
{
    public interface IOrderService
    {
        Task<int> CreateOrderAsync(OrderCreateViewModel model);
        Task<List<OrderViewModel>> GetOrdersAsync();
        Task<OrderDetailViewModel> GetOrderDetailAsync(int id);
    }
} 