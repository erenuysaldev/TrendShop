using ECommerce.Business.DTOs;
using ECommerce.Entity.Entities;
using ECommerce.Shared.Results;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.Business.Interfaces
{
    // Ürünlerle ilgili iş kurallarını tanımlayan interface
    public interface IProductService
    {
        // Tüm ürünleri getir
        Task<IDataResult<List<ProductDto>>> GetAllProducts();
        
        // ID'ye göre ürün getir
        Task<IDataResult<Product>> GetByIdAsync(int id);
        
        // Kategoriye göre ürünleri getir
        Task<IDataResult<List<ProductDto>>> GetProductsByCategory(int categoryId);
        
        // Yeni ürün ekle
        Task<IResult> AddProduct(ProductDto productDto);
        
        // Ürün güncelle
        Task<IResult> UpdateAsync(int id, CreateProductDto model);
        
        // Ürün sil
        Task<IResult> DeleteProduct(int id);

        // Yeni metodlar
        Task AddAsync(Product product);
        Task<IEnumerable<Product>> GetAllAsync();
    }
} 