using ECommerce.Business.DTOs;
using ECommerce.Entity.Entities;
using ECommerce.Shared.Results;

namespace ECommerce.Business.Interfaces
{
    public interface ICategoryService
    {
        // Tüm kategorileri getir
        Task<IDataResult<List<CategoryDto>>> GetAllCategories();
        
        // ID'ye göre kategori getir
        Task<IDataResult<CategoryDto>> GetCategoryById(int id);
        
        // Yeni kategori ekle
        Task<IResult> AddCategory(CategoryDto categoryDto);
        
        // Kategori güncelle
        Task<IResult> UpdateCategory(CategoryDto categoryDto);
        
        // Kategori sil
        Task<IResult> DeleteCategory(int id);

        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category> GetByIdAsync(int id);
        Task<Category> GetByNameAsync(string name);
        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id);
    }
} 