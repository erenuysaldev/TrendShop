namespace ECommerce.Web.Services
{
    public interface IApiService
    {
        Task<T> GetAsync<T>(string endpoint, string? token = null);
        Task<T> PostAsync<T>(string endpoint, object data, string? token = null);
        Task<T> PutAsync<T>(string endpoint, object data, string? token = null);
        Task<T> DeleteAsync<T>(string endpoint, string? token = null);
    }
} 