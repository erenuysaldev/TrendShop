using ECommerce.Business.DTOs;
using ECommerce.Data.Models;
using ECommerce.Shared.Results;

namespace ECommerce.Business.Interfaces
{
    public interface ICartService
    {
        Task<IDataResult<CartDto>> GetCart(string userId);
        Task<IResult> AddToCartAsync(string userId, int productId, int quantity);
        Task<IResult> UpdateCartItem(string userId, int productId, int quantity);
        Task<IResult> RemoveFromCart(string userId, int productId);
        Task<IResult> ClearCart(string userId);
        Task<List<CartViewModel>> GetCartAsync(string userId);
        Task UpdateQuantityAsync(string userId, int productId, int quantity);
        Task RemoveFromCartAsync(string userId, int productId);
    }
} 