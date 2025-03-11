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
                .Where(c => !c.IsDeleted)
                .ToListAsync();
        }

        public new async Task<Category> GetByIdAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public override async Task UpdateAsync(Category entity)
        {
            entity.UpdatedAt = DateTime.Now;
            _context.Categories.Update(entity);
            // SaveChangesAsync çağrısı Repository sınıfında yapılacak
        }

        public async Task<Category?> GetByNameAsync(string name)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower() && !c.IsDeleted);
        }
    }
} 