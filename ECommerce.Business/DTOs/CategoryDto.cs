namespace ECommerce.Business.DTOs
{
    // Kategori bilgilerini taşıyan DTO sınıfı
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        
        // Bu kategorideki ürün sayısı
        public int ProductCount { get; set; }
    }
} 