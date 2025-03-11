using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet("Ping")]
        public IActionResult Ping()
        {
            return Ok(new { message = "API is running", timestamp = DateTime.UtcNow });
        }
        
        [HttpGet("OrdersTest")]
        public IActionResult OrdersTest()
        {
            return Ok(new { 
                message = "Orders endpoint test", 
                routeInfo = new {
                    controllerExists = true,
                    expectedRoute = "/api/Order/Admin/GetAllOrders",
                    methodName = "GetAllOrders"
                }
            });
        }
    }
} 