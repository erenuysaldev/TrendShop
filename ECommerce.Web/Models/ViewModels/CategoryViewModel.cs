using System.ComponentModel.DataAnnotations;

namespace ECommerce.Web.Models.ViewModels
{
    public class CategoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kategori adı gereklidir")]
        [Display(Name = "Kategori Adı")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Açıklama")]
        public string? Description { get; set; }
    }
} 