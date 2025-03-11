namespace ECommerce.Business.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderNumber => Id.ToString("D8"); // 8 haneli sipariş numarası
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
} 