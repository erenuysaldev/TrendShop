namespace ECommerce.Entity.Entities
{
    public class Order : BaseEntity
    {
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending";
        public string ShippingAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string BillingAddress { get; set; }
        public string PaymentMethod { get; set; }
        public string OrderNotes { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
} 