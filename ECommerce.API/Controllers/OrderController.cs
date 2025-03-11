using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ECommerce.API.Models;
using ECommerce.Business.DTOs;
using ECommerce.Business.Interfaces;
using ECommerce.API.Filters;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] Business.DTOs.CreateOrderDto model)
        {
            try
            {
                _logger.LogInformation("Sipariş oluşturma isteği alındı: {@OrderInfo}", 
                    new { model.FullName, model.Email, model.PaymentMethod });
                
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Sipariş oluşturma başarısız: Kullanıcı kimliği bulunamadı");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Kullanıcı kimliği bulunamadı"
                    });
                }

                var result = await _orderService.CreateOrderAsync(userId, model);

                if (!result.Success)
                {
                    _logger.LogWarning("Sipariş oluşturma başarısız: {Message}", result.Message);
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = result.Message
                    });
                }

                _logger.LogInformation("Sipariş başarıyla oluşturuldu. Kullanıcı ID: {UserId}", userId);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Sipariş başarıyla oluşturuldu",
                    Data = new { id = 1 } // Gerçek sipariş ID'si burada döndürülmeli
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş oluşturulurken hata oluştu");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Sipariş oluşturulurken bir hata oluştu: " + ex.Message
                });
            }
        }
        
        [HttpGet("GetUserOrders")]
        public async Task<IActionResult> GetUserOrders()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Kullanıcı siparişleri getirilemedi: Kullanıcı kimliği bulunamadı");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Kullanıcı kimliği bulunamadı"
                    });
                }

                var result = await _orderService.GetUserOrdersAsync(userId);
                
                if (!result.Success)
                {
                    _logger.LogWarning("Kullanıcı siparişleri getirilemedi: {Message}", result.Message);
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = result.Message
                    });
                }
                
                _logger.LogInformation("Kullanıcı siparişleri başarıyla getirildi. Kullanıcı ID: {UserId}, Sipariş Sayısı: {OrderCount}", 
                    userId, result.Data.Count);

                return Ok(new ApiResponse<List<Business.DTOs.OrderDto>>
                {
                    Success = true,
                    Data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı siparişleri getirilirken hata oluştu");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Siparişler getirilirken bir hata oluştu: " + ex.Message
                });
            }
        }
        
        [HttpGet("GetOrderDetail/{id}")]
        public async Task<IActionResult> GetOrderDetail(int id)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Sipariş detayı getirilemedi: Kullanıcı kimliği bulunamadı");
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Kullanıcı kimliği bulunamadı"
                    });
                }

                var result = await _orderService.GetOrderDetailAsync(id, userId);
                
                if (!result.Success)
                {
                    _logger.LogWarning("Sipariş detayı bulunamadı. Sipariş ID: {OrderId}, Kullanıcı ID: {UserId}", 
                        id, userId);
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = result.Message
                    });
                }
                
                _logger.LogInformation("Sipariş detayı başarıyla getirildi. Sipariş ID: {OrderId}, Kullanıcı ID: {UserId}", 
                    id, userId);

                return Ok(new ApiResponse<Business.DTOs.OrderDetailDto>
                {
                    Success = true,
                    Data = result.Data
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş detayı getirilirken hata oluştu. Sipariş ID: {OrderId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Sipariş detayı getirilirken bir hata oluştu: " + ex.Message
                });
            }
        }
        
        [HttpGet("Admin/GetAllOrders")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrders()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            
            Console.WriteLine($"Admin/GetAllOrders - User: {userId} ({userEmail})");
            Console.WriteLine($"İstek HTTP Headerları:");
            foreach (var header in Request.Headers)
            {
                Console.WriteLine($"  {header.Key}: {header.Value}");
            }
            
            // Rolleri kontrol et
            Console.WriteLine("Kullanıcı rolleri:");
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"  {claim.Type}: {claim.Value}");
            }
            
            // Doğrudan rol kontrolü
            var isAdmin = User.IsInRole("Admin");
            Console.WriteLine($"IsInRole('Admin'): {isAdmin}");
            
            try
            {
                var result = await _orderService.GetAllOrdersAsync();
                Console.WriteLine($"Bulunan sipariş sayısı: {result.Data.Count}");
                return Ok(new ApiResponse<List<OrderDto>>
                {
                    Success = true,
                    Message = "Siparişler başarıyla getirildi",
                    Data = result.Data
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
                return StatusCode(500, new ApiResponse<string>
                {
                    Success = false,
                    Message = "Siparişler getirilirken bir hata oluştu",
                    Data = ex.Message
                });
            }
        }
        
        [HttpPut("Admin/UpdateOrderStatus/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto model)
        {
            try
            {
                // Kullanıcı bilgilerini log'la
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var email = User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
                var isAdmin = User.IsInRole("Admin");
                
                // Farklı rol talep türlerini kontrol edelim
                var hasAdminClaim = User.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == "Admin") || 
                                   User.HasClaim(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" && c.Value == "Admin") ||
                                   User.HasClaim(c => c.Type == "role" && c.Value == "Admin");
                
                // Tüm role tiplerini bir liste olarak alıp dahil edelim
                var roleClaims = User.Claims.Where(c => 
                    c.Type == ClaimTypes.Role || 
                    c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" || 
                    c.Type == "role")
                    .Select(c => c.Value)
                    .ToList();
                
                _logger.LogWarning("UpdateOrderStatus metoduna istek geldi. UserId: {UserId}, Email: {Email}", userId, email);
                _logger.LogWarning("IsInRole('Admin'): {IsAdmin}", isAdmin);
                _logger.LogWarning("HasAdminClaim: {HasAdminClaim}", hasAdminClaim);
                _logger.LogWarning("Roles: {Roles}", string.Join(", ", roleClaims));
                
                // Yetkilendirme kontrolü
                if (!hasAdminClaim && !roleClaims.Contains("Admin") && !isAdmin)
                {
                    _logger.LogWarning("Kullanıcı Admin rolüne sahip değil! UserId: {UserId}", userId);
                    return Forbid();
                }
                
                _logger.LogInformation("Sipariş durumu güncelleme isteği alındı. Sipariş ID: {OrderId}, Yeni Durum: {NewStatus}", 
                    id, model.Status);
                
                var result = await _orderService.UpdateOrderStatusAsync(id, model.Status);
                
                if (!result.Success)
                {
                    _logger.LogWarning("Sipariş durumu güncellenemedi. Sipariş ID: {OrderId}", id);
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = result.Message
                    });
                }
                
                _logger.LogInformation("Sipariş durumu başarıyla güncellendi. Sipariş ID: {OrderId}, Yeni Durum: {NewStatus}", 
                    id, model.Status);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Sipariş durumu başarıyla güncellendi"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş durumu güncellenirken hata oluştu. Sipariş ID: {OrderId}", id);
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Sipariş durumu güncellenirken bir hata oluştu: " + ex.Message
                });
            }
        }

        [HttpGet("Health")]
        [AllowAnonymous]
        public IActionResult Health()
        {
            return Ok(new 
            { 
                status = "healthy", 
                controller = "OrderController",
                time = DateTime.UtcNow,
                endpoints = new[]
                {
                    "/api/Order/Admin/GetAllOrders",
                    "/api/Order/{id}",
                    "/api/Order/Create",
                    "/api/Order/Admin/UpdateOrderStatus/{id}"
                }
            });
        }
    }
} 