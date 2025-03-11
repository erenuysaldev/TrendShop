using System.ComponentModel.DataAnnotations;

namespace ECommerce.Business.DTOs
{
    public class UpdateOrderStatusDto
    {
        [Required(ErrorMessage = "Durum bilgisi zorunludur")]
        public string Status { get; set; }
    }
} 