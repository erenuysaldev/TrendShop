namespace ECommerce.Entity.Entities
{
    using System.ComponentModel.DataAnnotations;

    public class Product : BaseEntity
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        public string? ImageUrl { get; set; }
        
        // Satıcı bilgisini tutacak alan
        public string? SellerId { get; set; }
        
        [Required]
        public int CategoryId { get; set; }
        public virtual Category Category { get; set; } = null!;
        
        // OrderItems collection'ı ekliyoruz
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        
        // Ürün resimleri koleksiyonu
        public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    }
} 