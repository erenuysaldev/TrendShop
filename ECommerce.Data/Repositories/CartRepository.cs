using Microsoft.EntityFrameworkCore;
using ECommerce.Data.Context;
using ECommerce.Data.Models;
using ECommerce.Entity.Entities;

namespace ECommerce.Data.Repositories
{
    public class CartRepository : Repository<Cart>
    {
        public CartRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Cart?> GetCartByUserId(string userId)
        {
            return await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId && !c.IsDeleted);
        }

        public async Task<CartItem?> GetCartItem(int cartId, int productId)
        {
            return await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cartId && 
                                         ci.ProductId == productId && 
                                         !ci.IsDeleted);
        }

        public void RemoveCartItem(CartItem cartItem)
        {
            try
            {
                Console.WriteLine($"Removing CartItem - Id: {cartItem.Id}, ProductId: {cartItem.ProductId}");
                _context.CartItems.Remove(cartItem);  // Soft delete yerine gerçek silme işlemi
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing CartItem: {ex}");
                throw;
            }
        }

        public async Task<List<CartViewModel>> GetCartAsync(string userId)
        {
            var cartItems = await _context.CartItems
                .Include(ci => ci.Product)
                .Where(ci => ci.Cart.UserId == userId)
                .Select(ci => new CartViewModel
                {
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    ImageUrl = ci.Product.ImageUrl,
                    Price = ci.Product.Price,
                    Quantity = ci.Quantity
                })
                .ToListAsync();

            return cartItems;
        }

        public async Task AddToCartAsync(string userId, int productId, int quantity)
        {
            var cart = await GetCartByUserId(userId);
            if (cart == null)
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) throw new Exception("Kullanıcı bulunamadı");

                cart = new Cart 
                { 
                    UserId = userId,
                    User = user  // User navigation property'sini ayarla
                };
                await _context.Carts.AddAsync(cart);
                await _context.SaveChangesAsync();
            }

            var cartItem = await GetCartItem(cart.Id, productId);
            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
                _context.CartItems.Update(cartItem);
            }
            else
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null) throw new Exception("Ürün bulunamadı");

                cartItem = new CartItem
                {
                    CartId = cart.Id,
                    Cart = cart,      // Cart navigation property'sini ayarla
                    ProductId = productId,
                    Product = product, // Product navigation property'sini ayarla
                    Quantity = quantity
                };
                await _context.CartItems.AddAsync(cartItem);
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateQuantityAsync(string userId, int productId, int quantity)
        {
            var cart = await GetCartByUserId(userId);
            if (cart == null) return;

            var cartItem = await GetCartItem(cart.Id, productId);
            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
                _context.CartItems.Update(cartItem);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveFromCartAsync(string userId, int productId)
        {
            var cart = await GetCartByUserId(userId);
            if (cart == null) return;

            var cartItem = await GetCartItem(cart.Id, productId);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearCartAsync(string userId)
        {
            var cart = await GetCartByUserId(userId);
            if (cart != null)
            {
                var cartItems = await _context.CartItems
                    .Where(ci => ci.CartId == cart.Id)
                    .ToListAsync();

                _context.CartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();
            }
        }
    }
} 