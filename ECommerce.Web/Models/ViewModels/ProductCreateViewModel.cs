using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Web.Models.ViewModels
{
    public class ProductCreateViewModel
    {
        [Required(ErrorMessage = "Ürün adı zorunludur")]
        public string Name { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Fiyat zorunludur")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stok miktarı zorunludur")]
        [Range(0, int.MaxValue, ErrorMessage = "Stok miktarı 0 veya daha büyük olmalıdır")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "Kategori seçimi zorunludur")]
        public int CategoryId { get; set; }

        public IFormFile? Image { get; set; }
    }
} 