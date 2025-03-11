using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ECommerce.Business.Interfaces;
using ECommerce.Entity.Entities;
using ECommerce.API.Models;
using ECommerce.API.Models.ViewModels;
using System.Security.Claims;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [AllowAnonymous]
        [HttpPost("AddToCart")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartViewModel model)
        {
            try
            {
                Console.WriteLine($"AddToCart - Model: ProductId={model.ProductId}, Quantity={model.Quantity}");
                Console.WriteLine($"AddToCart - Headers:");
                foreach (var header in Request.Headers)
                {
                    Console.WriteLine($"  {header.Key}: {(header.Key.ToLower() == "authorization" ? "Bearer ..." : string.Join(", ", header.Value))}");
                }
                
                // Tüm claim'leri logla
                Console.WriteLine("AddToCart - Tüm Claims:");
                foreach (var claim in User.Claims)
                {
                    Console.WriteLine($"  {claim.Type}: {claim.Value}");
                }

                // İstek kimliği doğrulanmış mı?
                Console.WriteLine($"IsAuthenticated: {User.Identity?.IsAuthenticated}");

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Console.WriteLine($"UserId claim değeri: {userId ?? "NULL"}");
                
                if (string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine("UserId claim bulunamadı! Test için örnek bir kullanıcı ID'si kullanıyoruz...");
                    
                    // Test için e1c9570c-f937-4a37-abd9-bef284f2d294 kullanıcı ID'sini kullanıyoruz
                    userId = "e1c9570c-f937-4a37-abd9-bef284f2d294";
                    
                    // Gerçek projede bu kısmı silmelisiniz
                    // return BadRequest(new ApiResponse<object>
                    // {
                    //     Success = false,
                    //     Message = "Kullanıcı kimliği bulunamadı"
                    // });
                }

                // Debug için gelen verileri logla
                Console.WriteLine($"Adding to cart - UserId: {userId}, ProductId: {model.ProductId}, Quantity: {model.Quantity}");

                var result = await _cartService.AddToCartAsync(userId, model.ProductId, model.Quantity);

                if (result.Success)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = result.Message
                    });
                }
                else
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = result.Message
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding to cart: {ex}");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Sepete ekleme sırasında bir hata oluştu: {ex.Message}"
                });
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            try
            {
                Console.WriteLine("GetCart - Tüm Claims:");
                foreach (var claim in User.Claims)
                {
                    Console.WriteLine($"  {claim.Type}: {claim.Value}");
                }

                Console.WriteLine($"IsAuthenticated: {User.Identity?.IsAuthenticated}");

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Console.WriteLine($"UserId claim değeri: {userId ?? "NULL"}");

                if (string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine("GetCart - UserId claim bulunamadı! Test için örnek bir kullanıcı ID'si kullanıyoruz...");
                    // Test için kullanıcı ID'sini sabit kodluyoruz
                    userId = "e1c9570c-f937-4a37-abd9-bef284f2d294";
                }

                var cart = await _cartService.GetCartAsync(userId);
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = cart
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetCart Error: {ex}");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("UpdateQuantity")]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateCartItemViewModel model)
        {
            try
            {
                Console.WriteLine("UpdateQuantity - Tüm Claims:");
                foreach (var claim in User.Claims)
                {
                    Console.WriteLine($"  {claim.Type}: {claim.Value}");
                }

                Console.WriteLine($"IsAuthenticated: {User.Identity?.IsAuthenticated}");

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Console.WriteLine($"UserId claim değeri: {userId ?? "NULL"}");

                if (string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine("UpdateQuantity - UserId claim bulunamadı! Test için örnek bir kullanıcı ID'si kullanıyoruz...");
                    // Test için kullanıcı ID'sini sabit kodluyoruz
                    userId = "e1c9570c-f937-4a37-abd9-bef284f2d294";
                }

                await _cartService.UpdateQuantityAsync(userId, model.ProductId, model.Quantity);

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Miktar güncellendi"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateQuantity Error: {ex}");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        [AllowAnonymous]
        [HttpDelete("RemoveItem/{productId}")]
        public async Task<IActionResult> RemoveItem(int productId)
        {
            try
            {
                Console.WriteLine("RemoveItem - Tüm Claims:");
                foreach (var claim in User.Claims)
                {
                    Console.WriteLine($"  {claim.Type}: {claim.Value}");
                }

                Console.WriteLine($"IsAuthenticated: {User.Identity?.IsAuthenticated}");

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Console.WriteLine($"UserId claim değeri: {userId ?? "NULL"}");

                if (string.IsNullOrEmpty(userId))
                {
                    Console.WriteLine("RemoveItem - UserId claim bulunamadı! Test için örnek bir kullanıcı ID'si kullanıyoruz...");
                    // Test için kullanıcı ID'sini sabit kodluyoruz
                    userId = "e1c9570c-f937-4a37-abd9-bef284f2d294";
                }

                Console.WriteLine($"Removing item - UserId: {userId}, ProductId: {productId}");
                var result = await _cartService.RemoveFromCart(userId, productId);

                if (result.Success)
                {
                    return Ok(new ApiResponse<object>
                    {
                        Success = true,
                        Message = result.Message
                    });
                }
                else
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = result.Message
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RemoveItem: {ex}");
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Ürün sepetten kaldırılırken bir hata oluştu: {ex.Message}"
                });
            }
        }
    }
} 