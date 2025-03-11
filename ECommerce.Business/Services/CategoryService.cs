using ECommerce.Business.DTOs;
using ECommerce.Business.Interfaces;
using ECommerce.Data.Repositories;
using ECommerce.Entity.Entities;
using ECommerce.Shared.Results;

namespace ECommerce.Business.Services
{
    public class CategoryService : ICategoryService
    {
        // Repository'i kullanabilmek için dependency injection
        private readonly CategoryRepository _categoryRepository;

        public CategoryService(CategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        // Yeni metodların implementasyonları
        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await _categoryRepository.GetAllAsync();
        }

        public async Task<Category> GetByIdAsync(int id)
        {
            return await _categoryRepository.GetByIdAsync(id);
        }

        public async Task<Category> GetByNameAsync(string name)
        {
            return await _categoryRepository.SingleOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
        }

        public async Task AddAsync(Category category)
        {
            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveChangesAsync();
        }

        public async Task UpdateAsync(Category category)
        {
            await _categoryRepository.UpdateAsync(category);
            await _categoryRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category != null)
            {
                _categoryRepository.Remove(category);
                await _categoryRepository.SaveChangesAsync();
            }
        }

        // Eski metodların implementasyonları
        public async Task<IDataResult<List<CategoryDto>>> GetAllCategories()
        {
            try
            {
                // Tüm kategorileri al
                var categories = await _categoryRepository.GetCategoriesWithProducts();

                // DTO'ya çevir
                var categoryDtos = categories.Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    // Ürün sayısını hesapla
                    ProductCount = c.Products.Count
                }).ToList();

                return new DataResult<List<CategoryDto>>(categoryDtos, true, "Kategoriler listelendi");
            }
            catch (Exception)
            {
                return new DataResult<List<CategoryDto>>(null, false, "Kategoriler listelenirken hata oluştu");
            }
        }

        public async Task<IDataResult<CategoryDto>> GetCategoryById(int id)
        {
            try
            {
                // ID'ye göre kategoriyi bul
                var category = await _categoryRepository.GetByIdAsync(id);

                if (category == null)
                {
                    return new DataResult<CategoryDto>(null, false, "Kategori bulunamadı");
                }

                // DTO'ya çevir
                var categoryDto = new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description
                };

                return new DataResult<CategoryDto>(categoryDto, true, "Kategori bulundu");
            }
            catch (Exception)
            {
                return new DataResult<CategoryDto>(null, false, "Kategori getirilirken hata oluştu");
            }
        }

        public async Task<IResult> AddCategory(CategoryDto categoryDto)
        {
            try
            {
                // Basit kontrol
                if (string.IsNullOrEmpty(categoryDto.Name))
                {
                    return new Result(false, "Kategori adı boş olamaz");
                }

                // Aynı isimde kategori var mı kontrol et
                var existingCategory = await _categoryRepository.SingleOrDefaultAsync(c => c.Name == categoryDto.Name);
                if (existingCategory != null)
                {
                    return new Result(false, "Bu isimde bir kategori zaten var");
                }

                // DTO'yu entity'e çevir
                var category = new Category
                {
                    Name = categoryDto.Name,
                    Description = categoryDto.Description
                };

                // Veritabanına ekle
                await _categoryRepository.AddAsync(category);
                await _categoryRepository.SaveChangesAsync();

                return new Result(true, "Kategori başarıyla eklendi");
            }
            catch (Exception)
            {
                return new Result(false, "Kategori eklenirken bir hata oluştu");
            }
        }

        public async Task<IResult> UpdateCategory(CategoryDto categoryDto)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(categoryDto.Id);
                if (category == null)
                    return new Result(false, "Kategori bulunamadı");

                category.Name = categoryDto.Name;
                category.Description = categoryDto.Description;

                await _categoryRepository.UpdateAsync(category);
                await _categoryRepository.SaveChangesAsync();

                return new Result(true, "Kategori başarıyla güncellendi");
            }
            catch (Exception ex)
            {
                return new Result(false, $"Güncelleme sırasında bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> DeleteCategory(int id)
        {
            try
            {
                // Kategoriyi bul
                var category = await _categoryRepository.GetByIdAsync(id);

                if (category == null)
                {
                    return new Result(false, "Silinecek kategori bulunamadı");
                }

                // Kategoride ürün var mı kontrol et
                var hasProducts = await _categoryRepository.AnyAsync(c => c.Id == id && c.Products.Any());
                if (hasProducts)
                {
                    return new Result(false, "Bu kategoride ürünler var. Önce ürünleri silmelisiniz");
                }

                // Sil (soft delete)
                _categoryRepository.Remove(category);
                await _categoryRepository.SaveChangesAsync();

                return new Result(true, "Kategori silindi");
            }
            catch (Exception)
            {
                return new Result(false, "Kategori silinirken hata oluştu");
            }
        }
    }
} 