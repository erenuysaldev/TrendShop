using System.ComponentModel.DataAnnotations;

namespace ECommerce.Web.Models
{
    public class ProductCreateModel
    {
        [Required(ErrorMessage = "Ürün adı gereklidir.")]
        [Display(Name = "Ürün Adı")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Açıklama alanı gereklidir.")]
        [Display(Name = "Ürün Açıklaması")]
        [MinLength(50, ErrorMessage = "Açıklama en az 50 karakter olmalıdır.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Fiyat gereklidir.")]
        [Range(0.01, 100000, ErrorMessage = "Fiyat 0'dan büyük olmalıdır.")]
        [Display(Name = "Fiyat")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stok miktarı gereklidir.")]
        [Range(1, 1000, ErrorMessage = "Stok miktarı 1 ile 1000 arasında olmalıdır.")]
        [Display(Name = "Stok Miktarı")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "Kategori seçilmelidir.")]
        [Display(Name = "Kategori")]
        public int CategoryId { get; set; }

        [Display(Name = "Ürün Resim URL'si")]
        [DataType(DataType.Url)]
        [Url(ErrorMessage = "Geçerli bir URL girin.")]
        public string ImageUrl { get; set; } = "https://via.placeholder.com/350";

        [Display(Name = "İndirimli Fiyat")]
        public decimal? DiscountedPrice { get; set; }

        [Display(Name = "İndirim Yüzdesi")]
        [Range(0, 99, ErrorMessage = "İndirim yüzdesi 0 ile 99 arasında olmalıdır.")]
        public int? DiscountPercentage { get; set; }

        [Display(Name = "Ürün Özellikleri")]
        public List<ProductFeature>? Features { get; set; }
    }

    public class ProductFeature
    {
        [Required(ErrorMessage = "Özellik adı gereklidir.")]
        [Display(Name = "Özellik Adı")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Özellik değeri gereklidir.")]
        [Display(Name = "Değer")]
        public string Value { get; set; }
    }
} 