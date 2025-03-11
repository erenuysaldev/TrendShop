using System;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Entity.Entities
{
    public class ProductImage : BaseEntity
    {
        [Required]
        public int ProductId { get; set; }
        
        [Required]
        public string ImageUrl { get; set; } = string.Empty;
        
        // Resim sıralaması için
        public int DisplayOrder { get; set; } = 0;
        
        // İlişkili ürün
        public virtual Product Product { get; set; } = null!;
        
        // Açıklama veya alternatif metin
        public string? AltText { get; set; }
        
        // Ana resim mi?
        public bool IsMain { get; set; } = false;
    }
} 