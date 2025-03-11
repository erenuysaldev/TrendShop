namespace ECommerce.Business.DTOs
{
    public class CartDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public List<CartItemDto> Items { get; set; }
        public decimal TotalAmount { get; set; }
    }
} 