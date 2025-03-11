using ECommerce.Business.DTOs;
using ECommerce.Business.Interfaces;
using ECommerce.Data.Repositories;
using ECommerce.Entity.Entities;
using ECommerce.Shared.Results;
using ECommerce.Shared.Constants;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ECommerce.Business.Services
{
    public class ProductService : IProductService
    {
        // Repository'i kullanabilmek için dependency injection
        private readonly ProductRepository _productRepository;
        private readonly CategoryRepository _categoryRepository;

        public ProductService(ProductRepository productRepository, CategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<IDataResult<List<ProductDto>>> GetAllProducts()
        {
            try
            {
                // Tüm ürünleri repository'den al
                var products = await _productRepository.GetProductsWithCategory();

                // Entity'leri DTO'ya çevir
                var productDtos = products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name ?? string.Empty,
                    Description = p.Description ?? string.Empty,
                    Price = p.Price,
                    Stock = p.Stock,
                    ImageUrl = p.ImageUrl ?? string.Empty,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category?.Name ?? string.Empty
                }).ToList();

                return new DataResult<List<ProductDto>>(productDtos, true);
            }
            catch (Exception ex)
            {
                return new DataResult<List<ProductDto>>(new List<ProductDto>(), false, ex.Message);
            }
        }

        public async Task<IDataResult<Product>> GetByIdAsync(int id)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                    return new DataResult<Product>(null, false, "Ürün bulunamadı");

                return new DataResult<Product>(product, true);
            }
            catch (Exception ex)
            {
                return new DataResult<Product>(null, false, ex.Message);
            }
        }

        public async Task<IDataResult<List<ProductDto>>> GetProductsByCategory(int categoryId)
        {
            try
            {
                var products = await _productRepository.GetProductsByCategory(categoryId);
                var productDtos = products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name ?? string.Empty,
                    Description = p.Description ?? string.Empty,
                    Price = p.Price,
                    Stock = p.Stock,
                    ImageUrl = p.ImageUrl ?? string.Empty,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category?.Name ?? string.Empty
                }).ToList();

                return new DataResult<List<ProductDto>>(productDtos, true, "Ürünler listelendi");
            }
            catch (Exception ex)
            {
                return new DataResult<List<ProductDto>>(new List<ProductDto>(), false, ex.Message);
            }
        }

        public async Task<IResult> AddProduct(ProductDto productDto)
        {
            try
            {
                if (!await ValidateProduct(new CreateProductDto 
                { 
                    Name = productDto.Name, 
                    Description = productDto.Description,
                    Price = productDto.Price,
                    Stock = productDto.Stock,
                    ImageUrl = productDto.ImageUrl,
                    CategoryId = productDto.CategoryId
                }))
                {
                    return new Result(false, "Geçersiz ürün bilgileri");
                }

                var product = new Product
                {
                    Name = productDto.Name,
                    Description = productDto.Description,
                    Price = productDto.Price,
                    Stock = productDto.Stock,
                    ImageUrl = productDto.ImageUrl,
                    CategoryId = productDto.CategoryId
                };

                await _productRepository.AddAsync(product);
                await _productRepository.SaveChangesAsync();

                return new Result(true, "Ürün başarıyla eklendi");
            }
            catch (Exception ex)
            {
                return new Result(false, $"Ürün eklenirken bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> UpdateAsync(int id, CreateProductDto model)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                    return new Result(false, "Ürün bulunamadı");

                if (!await ValidateProduct(model))
                    return new Result(false, "Geçersiz ürün bilgileri");

                product.Name = model.Name;
                product.Description = model.Description;
                product.Price = model.Price;
                product.Stock = model.Stock;
                product.ImageUrl = model.ImageUrl;
                product.CategoryId = model.CategoryId;

                await _productRepository.UpdateAsync(product);
                await _productRepository.SaveChangesAsync();

                return new Result(true, "Ürün başarıyla güncellendi");
            }
            catch (Exception ex)
            {
                return new Result(false, $"Güncelleme sırasında bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                    return new Result(false, "Silinecek ürün bulunamadı");

                _productRepository.Remove(product);
                await _productRepository.SaveChangesAsync();

                return new Result(true, "Ürün başarıyla silindi");
            }
            catch (Exception ex)
            {
                return new Result(false, $"Ürün silinirken bir hata oluştu: {ex.Message}");
            }
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _productRepository.GetAllAsync();
        }

        public async Task AddAsync(Product product)
        {
            await _productRepository.AddAsync(product);
            await _productRepository.SaveChangesAsync();
        }

        private async Task<bool> ValidateProduct(CreateProductDto model)
        {
            if (string.IsNullOrEmpty(model.Name) || 
                string.IsNullOrEmpty(model.Description) || 
                model.Price <= 0 || 
                model.Stock < 0)
                return false;

            var category = await _categoryRepository.GetByIdAsync(model.CategoryId);
            return category != null;
        }
    }
} 