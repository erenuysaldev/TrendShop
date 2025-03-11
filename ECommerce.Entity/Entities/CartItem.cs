namespace ECommerce.Entity.Entities
{
    public class CartItem : BaseEntity
    {
        public int CartId { get; set; }
        public required Cart Cart { get; set; }
        
        public int ProductId { get; set; }
        public required Product Product { get; set; }
        
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
} 