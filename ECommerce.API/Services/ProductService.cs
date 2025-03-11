using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ECommerce.Entity.Entities;
using ECommerce.Data;
using ECommerce.API.Services.Interfaces;
using ECommerce.Data.Context;

namespace ECommerce.API.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetSellerProducts(string sellerId)
        {
            return await _context.Products
                .Where(p => p.SellerId == sellerId && !p.IsDeleted)
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
    }
} 