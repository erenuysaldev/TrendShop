using ECommerce.Data.Context;
using ECommerce.Entity.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Data.Repositories
{
    public class CategoryRepository : Repository<Category>
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Category>> GetCategoriesWithProducts()
        {
            return await _context.Categories
                .Include(c => c.Products)
                .ToListAsync();
        }
    }
} 