namespace ECommerce.Web.Models.DTOs
{
    public class ApiResponseDto
    {
        public string Message { get; set; }
        public bool Success { get; set; }
        public object Data { get; set; }
    }
} 