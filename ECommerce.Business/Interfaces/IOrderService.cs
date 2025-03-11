using ECommerce.Business.DTOs;
using ECommerce.Shared.Results;

namespace ECommerce.Business.Interfaces
{
    public interface IOrderService
    {
        Task<IResult> CreateOrderAsync(string userId, CreateOrderDto model);
        Task<IDataResult<List<OrderDto>>> GetUserOrdersAsync(string userId);
        Task<IDataResult<OrderDetailDto>> GetOrderDetailAsync(int id, string userId);
        Task<IDataResult<List<OrderDto>>> GetAllOrdersAsync();
        Task<IResult> UpdateOrderStatusAsync(int id, string status);
    }
} 