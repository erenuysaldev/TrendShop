using Microsoft.EntityFrameworkCore;
using ECommerce.Data.Context;
using ECommerce.Entity.Entities;

namespace ECommerce.Data.Repositories
{
    public class OrderRepository : Repository<Order>
    {
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<Order>> GetUserOrdersAsync(string userId)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId && !o.IsDeleted)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }
        
        public async Task<Order> GetOrderDetailAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);
        }
        
        public async Task<List<OrderItem>> GetOrderItemsAsync(int orderId)
        {
            return await _context.OrderItems
                .Include(oi => oi.Product)
                .Where(oi => oi.OrderId == orderId)
                .ToListAsync();
        }
        
        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => !o.IsDeleted)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }
    }
} 