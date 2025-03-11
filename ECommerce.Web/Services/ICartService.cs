using ECommerce.Web.Models.ViewModels;

namespace ECommerce.Web.Services
{
    public interface ICartService
    {
        Task<List<CartViewModel>> GetCartItemsAsync();
        Task<bool> AddToCartAsync(int productId, int quantity);
        Task<bool> UpdateCartItemAsync(int productId, int quantity);
        Task<bool> RemoveFromCartAsync(int productId);
        Task<bool> ClearCartAsync();
    }
} 