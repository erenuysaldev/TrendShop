using System.ComponentModel.DataAnnotations;

namespace ECommerce.Web.Models.ViewModels
{
    public class CheckoutViewModel
    {
        // Adres Bilgileri
        [Required(ErrorMessage = "Ad Soyad zorunludur")]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; }
        
        [Required(ErrorMessage = "E-posta adresi zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [Display(Name = "E-posta")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Telefon numarası zorunludur")]
        [Display(Name = "Telefon Numarası")]
        public string PhoneNumber { get; set; }
        
        [Required(ErrorMessage = "Adres zorunludur")]
        [Display(Name = "Adres")]
        public string Address { get; set; }
        
        [Required(ErrorMessage = "İl zorunludur")]
        [Display(Name = "İl")]
        public string City { get; set; }
        
        [Required(ErrorMessage = "İlçe zorunludur")]
        [Display(Name = "İlçe")]
        public string District { get; set; }
        
        [Required(ErrorMessage = "Posta kodu zorunludur")]
        [Display(Name = "Posta Kodu")]
        public string ZipCode { get; set; }
        
        // Ödeme Bilgileri
        [Required(ErrorMessage = "Kart sahibi adı zorunludur")]
        [Display(Name = "Kart Sahibi")]
        public string CardHolderName { get; set; }
        
        [Required(ErrorMessage = "Kart numarası zorunludur")]
        [Display(Name = "Kart Numarası")]
        [RegularExpression(@"^\d{16}$", ErrorMessage = "Kart numarası 16 haneli olmalıdır")]
        public string CardNumber { get; set; }
        
        [Required(ErrorMessage = "Son kullanma ayı zorunludur")]
        [Display(Name = "Son Kullanma Ayı")]
        [Range(1, 12, ErrorMessage = "Geçerli bir ay giriniz (1-12)")]
        public int ExpiryMonth { get; set; }
        
        [Required(ErrorMessage = "Son kullanma yılı zorunludur")]
        [Display(Name = "Son Kullanma Yılı")]
        [Range(2023, 2035, ErrorMessage = "Geçerli bir yıl giriniz")]
        public int ExpiryYear { get; set; }
        
        [Required(ErrorMessage = "CVV zorunludur")]
        [Display(Name = "CVV")]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV 3 veya 4 haneli olmalıdır")]
        public string Cvv { get; set; }
        
        // Ödeme Yöntemi
        public string PaymentMethod { get; set; }
        
        // Fatura Bilgileri
        [Display(Name = "Fatura adresi teslimat adresi ile aynı")]
        public bool SameAsBillingAddress { get; set; } = true;
        
        [Display(Name = "Fatura Adresi")]
        public string BillingAddress { get; set; }
        
        [Display(Name = "Fatura İli")]
        public string BillingCity { get; set; }
        
        [Display(Name = "Fatura İlçesi")]
        public string BillingDistrict { get; set; }
        
        [Display(Name = "Fatura Posta Kodu")]
        public string BillingZipCode { get; set; }
        
        // Sipariş Özeti
        public List<CartViewModel> CartItems { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal ShippingCost { get; set; } = 29.90m;
        public decimal TotalWithShipping => TotalAmount + ShippingCost;
        
        // Sipariş Notları
        [Display(Name = "Sipariş Notu")]
        public string OrderNotes { get; set; }
    }
} 