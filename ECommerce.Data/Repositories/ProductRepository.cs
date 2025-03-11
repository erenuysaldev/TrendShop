using ECommerce.Data.Context;
using ECommerce.Entity.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Data.Repositories
{
    public class ProductRepository : Repository<Product>
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Product>> GetProductsWithCategory()
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => !p.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsByCategory(int categoryId)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == categoryId && !p.IsDeleted)
                .ToListAsync();
        }

        public new async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public override async Task UpdateAsync(Product entity)
        {
            entity.UpdatedAt = DateTime.Now;
            _context.Products.Update(entity);
        }
    }
} 