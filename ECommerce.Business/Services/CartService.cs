using ECommerce.Business.DTOs;
using ECommerce.Business.Interfaces;
using ECommerce.Data.Models;
using ECommerce.Data.Repositories;
using ECommerce.Entity.Entities;
using ECommerce.Shared.Results;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Business.Services
{
    public class CartService : ICartService
    {
        private readonly CartRepository _cartRepository;
        private readonly IProductService _productService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartService(
            CartRepository cartRepository, 
            IProductService productService,
            UserManager<ApplicationUser> userManager)
        {
            _cartRepository = cartRepository;
            _productService = productService;
            _userManager = userManager;
        }

        public async Task<IDataResult<CartDto>> GetCart(string userId)
        {
            try
            {
                // Sepeti bul veya oluştur
                var cart = await _cartRepository.GetCartByUserId(userId);
                
                // Sepet yoksa yeni oluştur
                if (cart == null)
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user == null)
                        return new DataResult<CartDto>(null, false, "Kullanıcı bulunamadı");

                    cart = new Cart 
                    { 
                        UserId = userId,
                        User = user
                    };
                    await _cartRepository.AddAsync(cart);
                    await _cartRepository.SaveChangesAsync();

                    // Boş sepet DTO'su döndür
                    var emptyCartDto = new CartDto
                    {
                        Id = cart.Id,
                        UserId = cart.UserId,
                        TotalAmount = 0,
                        Items = new List<CartItemDto>()
                    };

                    return new DataResult<CartDto>(emptyCartDto, true, "Yeni sepet oluşturuldu");
                }

                // Varolan sepeti DTO'ya çevir
                var cartDto = new CartDto
                {
                    Id = cart.Id,
                    UserId = cart.UserId,
                    TotalAmount = cart.TotalAmount,
                    Items = cart.CartItems.Select(ci => new CartItemDto
                    {
                        ProductId = ci.ProductId,
                        ProductName = ci.Product.Name,
                        Price = ci.Product.Price,
                        Quantity = ci.Quantity
                    }).ToList()
                };

                return new DataResult<CartDto>(cartDto, true, "Sepet getirildi");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sepet getirilirken hata: {ex.Message}"); // Hata logla
                return new DataResult<CartDto>(null, false, "Sepet getirilirken bir hata oluştu");
            }
        }

        public async Task<IResult> AddToCartAsync(string userId, int productId, int quantity)
        {
            try
            {
                var productResult = await _productService.GetByIdAsync(productId);
                if (!productResult.Success || productResult.Data == null)
                {
                    return new Result(false, "Ürün bulunamadı");
                }

                var product = productResult.Data;
                if (product.Stock < quantity)
                {
                    return new Result(false, "Yetersiz stok");
                }

                var cart = await _cartRepository.GetCartByUserId(userId);
                if (cart == null)
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user == null)
                    {
                        return new Result(false, "Kullanıcı bulunamadı");
                    }

                    cart = new Cart
                    {
                        UserId = userId,
                        User = user,
                        CartItems = new List<CartItem>()
                    };
                    await _cartRepository.AddAsync(cart);
                    await _cartRepository.SaveChangesAsync();
                }

                var cartItem = await _cartRepository.GetCartItem(cart.Id, productId);
                if (cartItem != null)
                {
                    cartItem.Quantity += quantity;
                }
                else
                {
                    cartItem = new CartItem
                    {
                        CartId = cart.Id,
                        Cart = cart,
                        ProductId = productId,
                        Product = product,
                        Quantity = quantity,
                        Price = product.Price
                    };
                    cart.CartItems.Add(cartItem);
                }

                await _cartRepository.SaveChangesAsync();
                return new Result(true, "Ürün sepete eklendi");
            }
            catch (Exception ex)
            {
                return new Result(false, $"Ürün sepete eklenirken bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> UpdateCartItem(string userId, int productId, int quantity)
        {
            try
            {
                var cart = await _cartRepository.GetCartByUserId(userId);
                if (cart == null)
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    if (user == null)
                        return new Result(false, "Kullanıcı bulunamadı");

                    cart = new Cart 
                    { 
                        UserId = userId,
                        User = user,
                        CartItems = new List<CartItem>()  // Initialize the CartItems collection
                    };
                    await _cartRepository.AddAsync(cart);
                    await _cartRepository.SaveChangesAsync();
                }

                var cartItem = await _cartRepository.GetCartItem(cart.Id, productId);
                if (cartItem == null)
                    return new Result(false, "Ürün sepette bulunamadı");

                if (quantity <= 0)
                {
                    return await RemoveFromCart(userId, productId);
                }

                // Stok kontrolü
                var product = await _productService.GetByIdAsync(productId);
                if (product.Data.Stock < quantity)
                    return new Result(false, "Yetersiz stok");

                cartItem.Quantity = quantity;
                await _cartRepository.SaveChangesAsync();

                return new Result(true, "Sepet güncellendi");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sepet güncellenirken hata: {ex.Message}"); // Hata logla
                return new Result(false, $"Sepet güncellenirken bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> RemoveFromCart(string userId, int productId)
        {
            try
            {
                Console.WriteLine($"Removing from cart - UserId: {userId}, ProductId: {productId}");
                
                var cart = await _cartRepository.GetCartByUserId(userId);
                if (cart == null)
                {
                    Console.WriteLine($"Cart not found for user: {userId}");
                    return new Result(false, "Sepet bulunamadı");
                }

                var cartItem = await _cartRepository.GetCartItem(cart.Id, productId);
                if (cartItem == null)
                {
                    Console.WriteLine($"CartItem not found - CartId: {cart.Id}, ProductId: {productId}");
                    return new Result(false, "Ürün sepette bulunamadı");
                }

                try
                {
                    _cartRepository.RemoveCartItem(cartItem);
                    await _cartRepository.SaveChangesAsync();
                    Console.WriteLine($"Successfully removed item from cart - CartId: {cart.Id}, ProductId: {productId}");
                    return new Result(true, "Ürün sepetten kaldırıldı");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error removing cart item: {ex}");
                    return new Result(false, "Ürün sepetten kaldırılırken bir hata oluştu: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RemoveFromCart: {ex}");
                return new Result(false, "Ürün sepetten kaldırılırken bir hata oluştu: " + ex.Message);
            }
        }

        public async Task<IResult> ClearCart(string userId)
        {
            try
            {
                var cart = await _cartRepository.GetCartByUserId(userId);
                if (cart == null)
                    return new Result(false, "Sepet bulunamadı");

                cart.CartItems.Clear();
                await _cartRepository.SaveChangesAsync();

                return new Result(true, "Sepet temizlendi");
            }
            catch (Exception)
            {
                return new Result(false, "Sepet temizlenirken bir hata oluştu");
            }
        }

        public async Task<List<CartViewModel>> GetCartAsync(string userId)
        {
            return await _cartRepository.GetCartAsync(userId);
        }

        public async Task UpdateQuantityAsync(string userId, int productId, int quantity)
        {
            await _cartRepository.UpdateQuantityAsync(userId, productId, quantity);
        }

        public async Task RemoveFromCartAsync(string userId, int productId)
        {
            await _cartRepository.RemoveFromCartAsync(userId, productId);
        }

        public async Task<IDataResult<Product>> GetByIdAsync(int id)
        {
            try
            {
                var product = await _productService.GetByIdAsync(id);
                if (!product.Success || product.Data == null)
                {
                    return new DataResult<Product>(default!, false, "Ürün bulunamadı");
                }

                return new DataResult<Product>(product.Data, true);
            }
            catch (Exception ex)
            {
                return new DataResult<Product>(default!, false, ex.Message);
            }
        }
    }
} 