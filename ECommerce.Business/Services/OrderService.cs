using ECommerce.Business.DTOs;
using ECommerce.Business.Interfaces;
using ECommerce.Data.Context;
using ECommerce.Data.Repositories;
using ECommerce.Entity.Entities;
using ECommerce.Shared.Results;

namespace ECommerce.Business.Services
{
    public class OrderService : IOrderService
    {
        private readonly OrderRepository _orderRepository;
        private readonly CartRepository _cartRepository;
        private readonly ProductRepository _productRepository;

        public OrderService(
            OrderRepository orderRepository,
            CartRepository cartRepository,
            ProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _cartRepository = cartRepository;
            _productRepository = productRepository;
        }

        public async Task<IResult> CreateOrderAsync(string userId, CreateOrderDto model)
        {
            try
            {
                // Sepeti al
                var cartItems = await _cartRepository.GetCartAsync(userId);
                if (!cartItems.Any())
                    return new Result(false, "Sepetiniz boş");

                // Siparişi oluştur
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    ShippingAddress = model.ShippingAddress,
                    PhoneNumber = model.PhoneNumber,
                    TotalAmount = cartItems.Sum(x => x.Price * x.Quantity),
                    Status = "Bekliyor",
                    FullName = model.FullName,
                    Email = model.Email,
                    BillingAddress = model.BillingAddress,
                    PaymentMethod = model.PaymentMethod,
                    OrderNotes = model.OrderNotes
                };

                // Sipariş detaylarını ekle
                foreach (var item in cartItems)
                {
                    var orderItem = new OrderItem
                    {
                        ProductId = item.ProductId,
                        Price = item.Price,
                        Quantity = item.Quantity
                    };
                    order.OrderItems.Add(orderItem);

                    // Stok güncelle
                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        product.Stock -= item.Quantity;
                        await _productRepository.UpdateAsync(product);
                    }
                }

                await _orderRepository.AddAsync(order);
                await _orderRepository.SaveChangesAsync();

                // Sepeti temizle
                await _cartRepository.ClearCartAsync(userId);

                return new Result(true, "Sipariş başarıyla oluşturuldu");
            }
            catch (Exception ex)
            {
                return new Result(false, $"Sipariş oluşturulurken bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IDataResult<List<OrderDto>>> GetUserOrdersAsync(string userId)
        {
            try
            {
                var orders = await _orderRepository.GetUserOrdersAsync(userId);
                var orderDtos = orders.Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    ShippingAddress = o.ShippingAddress,
                    PhoneNumber = o.PhoneNumber
                }).ToList();

                return new DataResult<List<OrderDto>>(orderDtos, true);
            }
            catch (Exception ex)
            {
                return new DataResult<List<OrderDto>>(new List<OrderDto>(), false, ex.Message);
            }
        }
        
        public async Task<IDataResult<OrderDetailDto>> GetOrderDetailAsync(int id, string userId)
        {
            try
            {
                var order = await _orderRepository.GetOrderDetailAsync(id);
                
                if (order == null)
                    return new DataResult<OrderDetailDto>(null, false, "Sipariş bulunamadı");
                
                // Kullanıcı kontrolü - sadece kendi siparişlerini görebilir (Admin hariç)
                if (order.UserId != userId)
                    return new DataResult<OrderDetailDto>(null, false, "Bu siparişi görüntüleme yetkiniz yok");
                
                var orderItems = await _orderRepository.GetOrderItemsAsync(id);
                
                var orderDetailDto = new OrderDetailDto
                {
                    Id = order.Id,
                    OrderNumber = order.Id.ToString("D8"), // 8 haneli sipariş numarası
                    OrderDate = order.OrderDate,
                    Status = order.Status,
                    TotalAmount = order.TotalAmount,
                    ShippingAddress = order.ShippingAddress,
                    BillingAddress = order.BillingAddress,
                    PhoneNumber = order.PhoneNumber,
                    Email = order.Email,
                    FullName = order.FullName,
                    PaymentMethod = order.PaymentMethod,
                    OrderNotes = order.OrderNotes,
                    Items = orderItems.Select(item => new OrderItemDto
                    {
                        ProductId = item.ProductId,
                        ProductName = item.Product?.Name ?? "Ürün Bulunamadı",
                        ImageUrl = item.Product?.ImageUrl ?? "/images/no-image.jpg",
                        Price = item.Price,
                        Quantity = item.Quantity
                    }).ToList()
                };
                
                return new DataResult<OrderDetailDto>(orderDetailDto, true);
            }
            catch (Exception ex)
            {
                return new DataResult<OrderDetailDto>(null, false, $"Sipariş detayı getirilirken bir hata oluştu: {ex.Message}");
            }
        }
        
        public async Task<IDataResult<List<OrderDto>>> GetAllOrdersAsync()
        {
            try
            {
                var orders = await _orderRepository.GetAllOrdersAsync();
                var orderDtos = orders.Select(o => new OrderDto
                {
                    Id = o.Id,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    ShippingAddress = o.ShippingAddress,
                    PhoneNumber = o.PhoneNumber,
                    FullName = o.FullName,
                    Email = o.Email
                }).ToList();

                return new DataResult<List<OrderDto>>(orderDtos, true);
            }
            catch (Exception ex)
            {
                return new DataResult<List<OrderDto>>(new List<OrderDto>(), false, $"Siparişler getirilirken bir hata oluştu: {ex.Message}");
            }
        }
        
        public async Task<IResult> UpdateOrderStatusAsync(int id, string status)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(id);
                
                if (order == null)
                    return new Result(false, "Sipariş bulunamadı");
                
                order.Status = status;
                await _orderRepository.UpdateAsync(order);
                await _orderRepository.SaveChangesAsync();
                
                return new Result(true, "Sipariş durumu başarıyla güncellendi");
            }
            catch (Exception ex)
            {
                return new Result(false, $"Sipariş durumu güncellenirken bir hata oluştu: {ex.Message}");
            }
        }
    }
} 