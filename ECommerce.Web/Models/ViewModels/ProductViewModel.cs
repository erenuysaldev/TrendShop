using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ECommerce.Web.Models.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ürün adı gereklidir.")]
        [Display(Name = "Ürün Adı")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Açıklama alanı gereklidir.")]
        [Display(Name = "Ürün Açıklaması")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Fiyat gereklidir.")]
        [Range(0.01, 100000, ErrorMessage = "Fiyat 0'dan büyük olmalıdır.")]
        [Display(Name = "Fiyat")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Stok miktarı gereklidir.")]
        [Range(0, 1000, ErrorMessage = "Stok miktarı 0 ile 1000 arasında olmalıdır.")]
        [Display(Name = "Stok Miktarı")]
        public int StockQuantity { get; set; }

        [Display(Name = "Stok")]
        public int Stock { 
            get { return StockQuantity; }
            set { StockQuantity = value; } 
        }

        [Required(ErrorMessage = "Kategori seçilmelidir.")]
        [Display(Name = "Kategori")]
        public int CategoryId { get; set; }

        [Display(Name = "Kategori")]
        public string CategoryName { get; set; }

        [Display(Name = "Ürün Resmi")]
        public string ImageUrl { get; set; }

        [Display(Name = "Ek Resimler")]
        public List<string> AdditionalImageUrls { get; set; }

        [Display(Name = "İndirimli Fiyat")]
        public decimal? DiscountedPrice { get; set; }

        [Display(Name = "İndirim Yüzdesi")]
        public int? DiscountPercentage { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Güncellenme Tarihi")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "Özellikler")]
        public List<ProductFeatureViewModel> Features { get; set; }
    }

    public class ProductFeatureViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Özellik adı gereklidir.")]
        [Display(Name = "Özellik Adı")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Özellik değeri gereklidir.")]
        [Display(Name = "Değer")]
        public string Value { get; set; }
    }
} 