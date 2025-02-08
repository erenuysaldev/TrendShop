using ECommerce.Business.DTOs;
using ECommerce.Shared.Results;

namespace ECommerce.Business.Interfaces
{
    // Ürünlerle ilgili iş kurallarını tanımlayan interface
    public interface IProductService
    {
        // Tüm ürünleri getir
        Task<IDataResult<List<ProductDto>>> GetAllProducts();
        
        // ID'ye göre ürün getir
        Task<IDataResult<ProductDto>> GetProductById(int id);
        
        // Kategoriye göre ürünleri getir
        Task<IDataResult<List<ProductDto>>> GetProductsByCategory(int categoryId);
        
        // Yeni ürün ekle
        Task<IResult> AddProduct(ProductDto productDto);
        
        // Ürün güncelle
        Task<IResult> UpdateProduct(ProductDto productDto);
        
        // Ürün sil
        Task<IResult> DeleteProduct(int id);
    }
} 