namespace ECommerce.Business.DTOs
{
    // DTO: Veritabanı entity'si yerine kullanıcıya gösterilecek veri modeli
    public class ProductDto
    {
        public int Id { get; set; }
        
        // Ürün adı
        public string Name { get; set; }
        
        // Ürün açıklaması
        public string Description { get; set; }
        
        // Ürün fiyatı
        public decimal Price { get; set; }
        
        // Stok miktarı
        public int Stock { get; set; }
        
        // Ürün resmi için URL
        public string ImageUrl { get; set; }
        
        // Ürünün kategorisi
        public string CategoryName { get; set; }
    }
} 