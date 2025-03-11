namespace ECommerce.Entity.Entities
{
    public class Cart : BaseEntity
    {
        public required string UserId { get; set; }
        public required ApplicationUser User { get; set; }
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public decimal TotalAmount => CartItems.Sum(ci => ci.Quantity * ci.Product.Price);
    }
} 