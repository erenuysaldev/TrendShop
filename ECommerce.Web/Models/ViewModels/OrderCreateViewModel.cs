using System.ComponentModel.DataAnnotations;

namespace ECommerce.Web.Models.ViewModels
{
    public class OrderCreateViewModel
    {
        [Required]
        public string FullName { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public string PhoneNumber { get; set; }
        
        [Required]
        public string ShippingAddress { get; set; }
        
        public string BillingAddress { get; set; }
        
        public string OrderNotes { get; set; }
        
        [Required]
        public string PaymentMethod { get; set; } // "CreditCard" veya "PayAtDoor"
        
        public CardInfoViewModel CardInfo { get; set; }
        
        [Required]
        public decimal TotalAmount { get; set; }
    }
    
    public class CardInfoViewModel
    {
        [Required]
        public string CardHolderName { get; set; }
        
        [Required]
        [StringLength(16, MinimumLength = 16)]
        public string CardNumber { get; set; }
        
        [Required]
        [Range(1, 12)]
        public int ExpiryMonth { get; set; }
        
        [Required]
        public int ExpiryYear { get; set; }
        
        [Required]
        [StringLength(4, MinimumLength = 3)]
        public string Cvv { get; set; }
    }
} 