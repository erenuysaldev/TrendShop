using System;
using System.Collections.Generic;

namespace ECommerce.Business.DTOs
{
    public class OrderDetailDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; }
        public string BillingAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string PaymentMethod { get; set; }
        public string OrderNotes { get; set; }
        public List<OrderItemDto> Items { get; set; }
    }

    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity;
    }
} 