using Microsoft.AspNetCore.Mvc;
using ECommerce.Business.Interfaces;
using ECommerce.Entity.Entities;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var categories = await _categoryService.GetAllAsync();
                return Ok(categories.Select(c => new
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description
                }).ToList());
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var category = await _categoryService.GetByIdAsync(id);
                if (category == null)
                    return NotFound();

                return Ok(category);
            }
            catch (Exception ex)
            {
                return BadRequest($"Kategori getirilirken bir hata oluştu: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryViewModel model)
        {
            try
            {
                var existingCategory = await _categoryService.GetByNameAsync(model.Name);
                if (existingCategory != null)
                {
                    return BadRequest(new { success = false, message = "Bu isimde bir kategori zaten var" });
                }

                var category = new Category
                {
                    Name = model.Name,
                    Description = model.Description
                };

                await _categoryService.AddAsync(category);
                return Ok(new { success = true, message = "Kategori başarıyla eklendi" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Category category)
        {
            try
            {
                if (id != category.Id)
                    return BadRequest("ID'ler eşleşmiyor");

                await _categoryService.UpdateAsync(category);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Kategori güncellenirken bir hata oluştu: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _categoryService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Kategori silinirken bir hata oluştu: {ex.Message}");
            }
        }
    }
} 