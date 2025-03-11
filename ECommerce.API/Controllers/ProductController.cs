using ECommerce.Business.DTOs;
using ECommerce.Business.Interfaces;
using ECommerce.Entity.Entities;
using Microsoft.AspNetCore.Mvc;
using ECommerce.API.Models.ViewModels;
using ECommerce.API.Models;
using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using ECommerce.API.Services.Interfaces;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        // Service'i kullanabilmek için dependency injection
        private readonly ECommerce.API.Services.Interfaces.IProductService _apiProductService;
        private readonly ILogger<ProductController> _logger;
        
        // Business katmanındaki IProductService'e erişmek için
        private readonly ECommerce.Business.Interfaces.IProductService _businessProductService;

        public ProductController(
            ECommerce.API.Services.Interfaces.IProductService apiProductService, 
            ECommerce.Business.Interfaces.IProductService businessProductService,
            ILogger<ProductController> logger)
        {
            _apiProductService = apiProductService;
            _businessProductService = businessProductService;
            _logger = logger;
        }

        // Tüm ürünleri getir
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _businessProductService.GetAllProducts();
                return Ok(new ApiResponse<List<ProductDto>>
                {
                    Success = true,
                    Message = "Ürünler başarıyla getirildi",
                    Data = result.Data
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<List<ProductDto>>
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null
                });
            }
        }

        // ID'ye göre ürün getir
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                Console.WriteLine($"GetById çağrıldı: id={id}");
                var result = await _businessProductService.GetByIdAsync(id);

                if (result.Success && result.Data != null)
                {
                    Console.WriteLine($"Ürün bulundu: {result.Data.Name}");
                    
                    // Ürün bilgilerini DTO'ya manuel olarak kopyalayıp,
                    // döngüsel referans olmadan döndürelim
                    var productDto = new ProductDto
                    {
                        Id = result.Data.Id,
                        Name = result.Data.Name,
                        Description = result.Data.Description,
                        Price = result.Data.Price,
                        Stock = result.Data.Stock,
                        ImageUrl = result.Data.ImageUrl,
                        CategoryId = result.Data.CategoryId,
                        CategoryName = result.Data.Category?.Name ?? "Bilinmeyen Kategori"
                    };
                    
                    return Ok(productDto);
                }
                
                Console.WriteLine($"Ürün bulunamadı: {result.Message}");
                return NotFound(new { message = result.Message ?? "Ürün bulunamadı" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetById error: {ex.Message}");
                return BadRequest(new { message = $"Ürün getirme sırasında bir hata oluştu: {ex.Message}" });
            }
        }

        // Kategoriye göre ürünleri getir
        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            var result = await _businessProductService.GetProductsByCategory(categoryId);
            
            if (result.Success)
                return Ok(result);
            
            return BadRequest(result);
        }

        // Yeni ürün ekle
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new { success = false, message = string.Join(", ", errors) });
                }

                // Gelen verileri log'la
                Console.WriteLine($"API'ye gelen ProductCreate verisi: Name={model.Name}, Price={model.Price}, CategoryId={model.CategoryId}, ImageUrl={model.ImageUrl ?? "Boş"}");

                // ImageUrl kontrolü
                string imageUrl;
                if (!string.IsNullOrWhiteSpace(model.ImageUrl))
                {
                    // Eğer URL http:// veya https:// ile başlıyorsa tam URL'dir
                    if (model.ImageUrl.StartsWith("http://") || model.ImageUrl.StartsWith("https://"))
                    {
                        imageUrl = model.ImageUrl; // URL'yi olduğu gibi kullan
                    }
                    else
                    {
                        // Relatif bir path ise varsayılan path ile birleştir
                        imageUrl = model.ImageUrl;
                    }
                }
                else
                {
                    // Varsayılan resim
                    imageUrl = "/images/no-image.jpg";
                }

                var product = new Product
                {
                    Name = model.Name?.Trim() ?? "",
                    Description = model.Description?.Trim() ?? "",
                    Price = model.Price,
                    Stock = model.Stock,
                    CategoryId = model.CategoryId,
                    // Belirlenen resim URL'sini kullan
                    ImageUrl = imageUrl
                };

                await _businessProductService.AddAsync(product);
                return Ok(new { success = true, message = "Ürün başarıyla eklendi" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating product: {ex}");
                return BadRequest(new { success = false, message = "Ürün eklenirken bir hata oluştu: " + ex.Message });
            }
        }

        // Ürün güncelle
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] ProductDto productDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                Console.WriteLine($"Update request - Id: {productDto.Id}, Name: {productDto.Name}, ImageUrl: {productDto.ImageUrl ?? "Boş"}");

                // Mevcut ürünü al
                var existingProduct = await _businessProductService.GetByIdAsync(productDto.Id);
                if (!existingProduct.Success || existingProduct.Data == null)
                {
                    return NotFound(new { success = false, message = "Güncellenecek ürün bulunamadı" });
                }

                // ImageUrl kontrolü
                string imageUrl;
                if (!string.IsNullOrWhiteSpace(productDto.ImageUrl))
                {
                    // Eğer URL http:// veya https:// ile başlıyorsa tam URL'dir
                    if (productDto.ImageUrl.StartsWith("http://") || productDto.ImageUrl.StartsWith("https://"))
                    {
                        imageUrl = productDto.ImageUrl; // URL'yi olduğu gibi kullan
                    }
                    else
                    {
                        // Relatif bir path ise olduğu gibi bırak
                        imageUrl = productDto.ImageUrl;
                    }
                }
                else
                {
                    // Mevcut resmi koru
                    imageUrl = existingProduct.Data.ImageUrl;
                }

                var result = await _businessProductService.UpdateAsync(productDto.Id, new CreateProductDto
                {
                    Name = productDto.Name,
                    Description = productDto.Description,
                    Price = productDto.Price,
                    Stock = productDto.Stock,
                    CategoryId = productDto.CategoryId,
                    ImageUrl = imageUrl
                });

                if (result.Success)
                    return Ok(result);

                return BadRequest(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating product: {ex}");
                return BadRequest(new { success = false, message = "Ürün güncellenirken bir hata oluştu: " + ex.Message });
            }
        }

        // Ürün sil
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _businessProductService.DeleteProduct(id);
            
            if (result.Success)
                return Ok(result);
            
            return BadRequest(result);
        }

        // Satıcıya ait ürünleri getiren endpoint
        [HttpGet("Seller/Products")]
        // [Authorize] - Yetkilendirme kontrolü kaldırıldı, herkes erişebilir
        public async Task<IActionResult> GetSellerProducts()
        {
            try
            {
                // Debug için kullanıcı bilgilerini ve claim'leri logla
                Console.WriteLine("GetSellerProducts çağrıldı - Herkes erişebilir");
                
                // Kullancı ID kontrolünü kaldırıyoruz, varsayılan bir dizi dönelim
                // Her erişimde boş bir liste dönüyor, gerçek bir uygulamada veri getirirsiniz
                return Ok(new ECommerce.API.Models.ApiResponse 
                { 
                    Success = true, 
                    Message = "Ürünler başarıyla getirildi.", 
                    Data = new List<object>() // Boş liste dönüyoruz
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ECommerce.API.Models.ApiResponse
                {
                    Success = false,
                    Message = "Ürünleri getirirken bir hata oluştu: " + ex.Message
                });
            }
        }
    }
} 