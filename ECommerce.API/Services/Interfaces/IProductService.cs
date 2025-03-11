using System.Collections.Generic;
using System.Threading.Tasks;
using ECommerce.Entity.Entities;

namespace ECommerce.API.Services.Interfaces
{
    public interface IProductService
    {
        /// <summary>
        /// Satıcıya ait ürünleri getirir
        /// </summary>
        /// <param name="sellerId">Satıcı ID</param>
        /// <returns>Satıcıya ait ürünlerin listesi</returns>
        Task<IEnumerable<Product>> GetSellerProducts(string sellerId);
    }
} 