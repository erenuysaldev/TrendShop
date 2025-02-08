using ECommerce.Business.DTOs;
using ECommerce.Business.Interfaces;
using ECommerce.Data.Repositories;
using ECommerce.Entity.Entities;
using ECommerce.Shared.Results;
using ECommerce.Shared.Constants;

namespace ECommerce.Business.Services
{
    public class ProductService : IProductService
    {
        // Repository'i kullanabilmek için dependency injection
        private readonly ProductRepository _productRepository;

        public ProductService(ProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IDataResult<List<ProductDto>>> GetAllProducts()
        {
            try
            {
                // Tüm ürünleri repository'den al
                var products = await _productRepository.GetProductsWithCategory();

                // Entity'leri DTO'ya çevir (şimdilik manuel mapping yapıyoruz)
                var productDtos = products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    ImageUrl = p.ImageUrl,
                    CategoryName = p.Category?.Name
                }).ToList();

                // Başarılı sonuç döndür
                return new DataResult<List<ProductDto>>(productDtos, true, Messages.Common.RecordFound);
            }
            catch (Exception)
            {
                // Hata durumunda boş liste döndür
                return new DataResult<List<ProductDto>>(null, false, "Ürünler getirilirken bir hata oluştu");
            }
        }

        // Diğer metodları da benzer şekilde implement edeceğiz...
        public Task<IDataResult<ProductDto>> GetProductById(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IDataResult<List<ProductDto>>> GetProductsByCategory(int categoryId)
        {
            throw new NotImplementedException();
        }

        public Task<IResult> AddProduct(ProductDto productDto)
        {
            throw new NotImplementedException();
        }

        public Task<IResult> UpdateProduct(ProductDto productDto)
        {
            throw new NotImplementedException();
        }

        public Task<IResult> DeleteProduct(int id)
        {
            throw new NotImplementedException();
        }
    }
} 