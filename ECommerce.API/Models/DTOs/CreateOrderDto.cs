using System.ComponentModel.DataAnnotations;

namespace ECommerce.API.Models.DTOs
{
    public class CreateOrderDto
    {
        [Required(ErrorMessage = "Ad Soyad zorunludur")]
        public string FullName { get; set; }
        
        [Required(ErrorMessage = "E-posta adresi zorunludur")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        public string Email { get; set; }
        
        [Required(ErrorMessage = "Teslimat adresi zorunludur")]
        public string ShippingAddress { get; set; }
        
        [Required(ErrorMessage = "Fatura adresi zorunludur")]
        public string BillingAddress { get; set; }
        
        [Required(ErrorMessage = "Telefon numarası zorunludur")]
        public string PhoneNumber { get; set; }
        
        public string OrderNotes { get; set; }
        
        [Required(ErrorMessage = "Ödeme yöntemi zorunludur")]
        public string PaymentMethod { get; set; } // "CreditCard" veya "PayAtDoor"
        
        public CardInfoDto CardInfo { get; set; }
        
        [Required(ErrorMessage = "Toplam tutar zorunludur")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Toplam tutar 0'dan büyük olmalıdır")]
        public decimal TotalAmount { get; set; }
    }
    
    public class CardInfoDto
    {
        [Required(ErrorMessage = "Kart sahibi adı zorunludur")]
        public string CardHolderName { get; set; }
        
        [Required(ErrorMessage = "Kart numarası zorunludur")]
        [RegularExpression(@"^\d{16}$", ErrorMessage = "Kart numarası 16 haneli olmalıdır")]
        public string CardNumber { get; set; }
        
        [Required(ErrorMessage = "Son kullanma ayı zorunludur")]
        [Range(1, 12, ErrorMessage = "Geçerli bir ay giriniz (1-12)")]
        public int ExpiryMonth { get; set; }
        
        [Required(ErrorMessage = "Son kullanma yılı zorunludur")]
        [Range(2023, 2035, ErrorMessage = "Geçerli bir yıl giriniz")]
        public int ExpiryYear { get; set; }
        
        [Required(ErrorMessage = "CVV zorunludur")]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV 3 veya 4 haneli olmalıdır")]
        public string Cvv { get; set; }
    }
} 