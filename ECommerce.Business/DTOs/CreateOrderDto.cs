using System.ComponentModel.DataAnnotations;

namespace ECommerce.Business.DTOs
{
    public class CreateOrderDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string BillingAddress { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string OrderNotes { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public CardInfoDto CardInfo { get; set; }
        public decimal TotalAmount { get; set; }
    }
    
    public class CardInfoDto
    {
        public string CardHolderName { get; set; } = string.Empty;
        public string CardNumber { get; set; } = string.Empty;
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }
        public string Cvv { get; set; } = string.Empty;
    }
} 