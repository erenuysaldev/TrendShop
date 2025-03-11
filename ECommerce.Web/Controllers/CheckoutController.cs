using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ECommerce.Web.Services;
using ECommerce.Web.Models.ViewModels;
using System.Text.Json;

namespace ECommerce.Web.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class CheckoutController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(ICartService cartService, IOrderService orderService, ILogger<CheckoutController> logger)
        {
            _cartService = cartService;
            _orderService = orderService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var cartItems = await _cartService.GetCartItemsAsync();
                if (cartItems == null || !cartItems.Any())
                {
                    return RedirectToAction("Index", "Cart");
                }

                var model = new CheckoutViewModel
                {
                    CartItems = cartItems,
                    TotalAmount = cartItems.Sum(item => item.Price * item.Quantity),
                    ShippingCost = 29.90m, // Sabit kargo ücreti
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Checkout sayfası yüklenirken hata oluştu");
                TempData["ErrorMessage"] = "Sipariş sayfası yüklenirken bir hata oluştu. Lütfen daha sonra tekrar deneyin.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [Route("PlaceOrder")]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            try
            {
                _logger.LogInformation("Sipariş oluşturma isteği alındı: {@Model}", new 
                { 
                    model.FullName,
                    model.Email,
                    model.PhoneNumber,
                    model.Address,
                    model.City,
                    PaymentMethod = model.CardNumber != null ? "CreditCard" : "PayAtDoor"
                });

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Sipariş formu doğrulama hatası: {@Errors}", 
                        ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    
                    // Sepet öğelerini yeniden yükle
                    model.CartItems = await _cartService.GetCartItemsAsync();
                    model.TotalAmount = model.CartItems.Sum(item => item.Price * item.Quantity);
                    model.ShippingCost = 29.90m;
                    
                    return View("Index", model);
                }

                // Fatura adresi teslimat adresi ile aynı ise
                if (model.SameAsBillingAddress)
                {
                    model.BillingAddress = model.Address;
                    model.BillingCity = model.City;
                    model.BillingDistrict = model.District;
                    model.BillingZipCode = model.ZipCode;
                }

                // Sipariş oluştur
                var orderRequest = new OrderCreateViewModel
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    ShippingAddress = $"{model.Address}, {model.District}, {model.City}, {model.ZipCode}",
                    BillingAddress = $"{model.BillingAddress}, {model.BillingDistrict}, {model.BillingCity}, {model.BillingZipCode}",
                    OrderNotes = model.OrderNotes,
                    PaymentMethod = model.CardNumber != null ? "CreditCard" : "PayAtDoor",
                    CardInfo = model.CardNumber != null ? new CardInfoViewModel
                    {
                        CardHolderName = model.CardHolderName,
                        CardNumber = model.CardNumber,
                        ExpiryMonth = model.ExpiryMonth,
                        ExpiryYear = model.ExpiryYear,
                        Cvv = model.Cvv
                    } : null,
                    TotalAmount = model.TotalWithShipping
                };

                var orderId = await _orderService.CreateOrderAsync(orderRequest);
                
                if (orderId > 0)
                {
                    _logger.LogInformation("Sipariş başarıyla oluşturuldu. Sipariş ID: {OrderId}", orderId);
                    
                    // Sepeti temizle
                    await _cartService.ClearCartAsync();
                    
                    // Sipariş onay sayfasına yönlendir
                    TempData["OrderId"] = orderId;
                    TempData["OrderTotal"] = model.TotalWithShipping.ToString("C");
                    return RedirectToAction("OrderConfirmation");
                }
                else
                {
                    _logger.LogWarning("Sipariş oluşturulamadı");
                    ModelState.AddModelError("", "Sipariş oluşturulurken bir hata oluştu. Lütfen daha sonra tekrar deneyin.");
                    
                    // Sepet öğelerini yeniden yükle
                    model.CartItems = await _cartService.GetCartItemsAsync();
                    model.TotalAmount = model.CartItems.Sum(item => item.Price * item.Quantity);
                    model.ShippingCost = 29.90m;
                    
                    return View("Index", model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş oluşturulurken hata oluştu");
                ModelState.AddModelError("", "Sipariş oluşturulurken bir hata oluştu. Lütfen daha sonra tekrar deneyin.");
                
                // Sepet öğelerini yeniden yükle
                model.CartItems = await _cartService.GetCartItemsAsync();
                model.TotalAmount = model.CartItems.Sum(item => item.Price * item.Quantity);
                model.ShippingCost = 29.90m;
                
                return View("Index", model);
            }
        }

        [HttpGet]
        [Route("OrderConfirmation")]
        public IActionResult OrderConfirmation()
        {
            var orderId = TempData["OrderId"];
            var orderTotal = TempData["OrderTotal"];
            
            if (orderId == null)
            {
                return RedirectToAction("Index", "Home");
            }
            
            ViewBag.OrderId = orderId;
            ViewBag.OrderTotal = orderTotal;
            
            return View();
        }
    }
} 