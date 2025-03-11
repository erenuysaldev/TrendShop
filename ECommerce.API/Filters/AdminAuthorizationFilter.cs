using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace ECommerce.API.Filters
{
    public class AdminAuthorizationFilter : IAuthorizationFilter
    {
        private readonly ILogger<AdminAuthorizationFilter> _logger;

        public AdminAuthorizationFilter(ILogger<AdminAuthorizationFilter> logger)
        {
            _logger = logger;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // ÖNEML: Debug için tüm oturum bilgilerini logla
            _logger.LogWarning("AdminAuthorizationFilter çalıştı - IsAuthenticated: {IsAuthenticated}", 
                context.HttpContext.User.Identity?.IsAuthenticated);
            
            var userId = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            _logger.LogWarning("UserId: {UserId}", userId ?? "NULL");
            
            // Tüm talepleri loglayalım
            foreach (var claim in context.HttpContext.User.Claims)
            {
                _logger.LogWarning("Claim: {Type} = {Value}", claim.Type, claim.Value);
            }
            
            // DEBUGGİNG İÇİN GEÇİCİ OLARAK YETKİLENDİRMEYİ ATLA
            return; // Tüm kontrolleri atla ve isteğe izin ver
        }
    }
} 