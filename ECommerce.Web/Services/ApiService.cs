using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.IO;
using ECommerce.Web.Models.ViewModels;
using Microsoft.Extensions.Logging;

namespace ECommerce.Web.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ApiService> _logger;

        public ApiService(HttpClient httpClient, IConfiguration configuration, ILogger<ApiService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            
            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            if (!string.IsNullOrEmpty(baseUrl))
            {
                _httpClient.BaseAddress = new Uri(baseUrl);
            }
            else
            {
                throw new ArgumentNullException("ApiSettings:BaseUrl", "API base URL is not configured");
            }
        }

        private void AddAuthorizationHeader(string? token)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    // Token işlemlerini detaylı loglama
                    _logger.LogInformation($"Token ekleniyor (ilk 20 karakter): {token.Substring(0, Math.Min(20, token.Length))}...");
                    
                    // Token format kontrolü
                    if (token.Contains('.') && token.Count(c => c == '.') >= 2)
                    {
                        // JWT formatında görünüyor, Bearer prefix kontrolü
                        if (!token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogInformation("Bearer öneki ekleniyor");
                            token = "Bearer " + token;
                        }
                        else
                        {
                            _logger.LogInformation("Token zaten Bearer önekine sahip");
                        }
                        
                        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", token);
                        _logger.LogInformation($"Authorization header eklendi: {token.Substring(0, Math.Min(20, token.Length))}...");
                    }
                    else
                    {
                        _logger.LogWarning("Token JWT formatında değil! Token: {Token}", token);
                    }
                    
                    // Tüm request headerlarını logla
                    _logger.LogDebug("Request Headers:");
                    foreach (var header in _httpClient.DefaultRequestHeaders)
                    {
                        _logger.LogDebug($"{header.Key}: {string.Join(", ", header.Value)}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Authorization header eklenirken hata oluştu");
                }
            }
            else
            {
                _logger.LogWarning("Token boş veya null olduğu için Authorization header'ı eklenmedi!");
            }
        }

        public async Task<T> GetAsync<T>(string endpoint, string? token = null)
        {
            try
            {
                _logger.LogInformation($"GetAsync başladı - Endpoint: {endpoint}, Token durumu: {(string.IsNullOrEmpty(token) ? "Boş" : "Var")}");
                
                // Authorization header'ı ekle
                AddAuthorizationHeader(token);
                
                _logger.LogInformation($"İstek yapılıyor: {_httpClient.BaseAddress}{endpoint}");
                var response = await _httpClient.GetAsync(endpoint);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                // Status code log için kullanılacak status ismi
                string statusName = response.StatusCode.ToString();
                _logger.LogInformation($"API yanıtı - Status: {statusName}, Content (ilk 100 karakter): {responseContent.Substring(0, Math.Min(100, responseContent.Length))}...");
                
                if (!response.IsSuccessStatusCode)
                {
                    // 401 Unauthorized hatası için boş sonuç
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        _logger.LogWarning("401 Unauthorized hatası alındı!");
                        _logger.LogWarning($"Gönderilen token: {(string.IsNullOrEmpty(token) ? "BOŞ" : token.Substring(0, Math.Min(20, token.Length)) + "...")}");
                        
                        // Eğer List veya IEnumerable türü ise boş liste döndür
                        if (typeof(T).IsGenericType && 
                            (typeof(T).GetGenericTypeDefinition() == typeof(List<>) || 
                             typeof(T).GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                             typeof(T).GetGenericTypeDefinition() == typeof(ICollection<>)))
                        {
                            _logger.LogInformation("Liste türü için boş sonuç döndürülüyor");
                            // Boş bir liste oluşturup Type casting ile döndürelim
                            var emptyList = Activator.CreateInstance(typeof(T));
                            return (T)emptyList;
                        }
                        
                        // Diğer türler için istisna fırlat
                        throw new UnauthorizedAccessException("Bu işlemi gerçekleştirmek için yetkiniz bulunmamaktadır veya oturumunuz sona ermiştir.");
                    }
                    
                    _logger.LogError($"API Error - Status: {response.StatusCode}, Content: {responseContent}");
                    throw new HttpRequestException($"API Error: {response.StatusCode} - {responseContent}");
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                try 
                {
                    var resultType = typeof(T);
                    
                    // Doğrudan bir JSON dizisi döndürüldüğünde işleyin (koleksiyon türleri için)
                    if (resultType.IsGenericType && 
                        (resultType.GetGenericTypeDefinition() == typeof(List<>) || 
                         resultType.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                         resultType.GetGenericTypeDefinition() == typeof(ICollection<>)))
                    {
                        try 
                        {
                            using (JsonDocument doc = JsonDocument.Parse(responseContent))
                            {
                                // Yanıt bir dizi mi?
                                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                                {
                                    Console.WriteLine("API bir dizi döndürdü, doğrudan ayrıştırılıyor");
                                    return JsonSerializer.Deserialize<T>(responseContent, options);
                                }
                            }
                        }
                        catch (JsonException jex)
                        {
                            Console.WriteLine($"JSON yapısı kontrol edilirken hata oluştu: {jex.Message}");
                        }
                    }
                    
                    // Eğer tip ApiResponse<something> değilse ve API yanıtı success/message/data formatındaysa
                    if (!resultType.IsGenericType || resultType.GetGenericTypeDefinition() != typeof(ApiResponse<>))
                    {
                        // ApiResponse<T> formatında olup olmadığını kontrol et
                        try
                        {
                            using (JsonDocument doc = JsonDocument.Parse(responseContent))
                            {
                                var root = doc.RootElement;
                                if (root.TryGetProperty("success", out _) && 
                                    root.TryGetProperty("data", out var dataProperty))
                                {
                                    // Eğer API bir ApiResponse döndürüyorsa ve biz sadece data'yı istiyorsak
                                    var dataJson = dataProperty.GetRawText();
                                    try
                                    {
                                        Console.WriteLine($"API bir ApiResponse döndürdü, data özelliği çözümleniyor: {dataJson}");
                                        return JsonSerializer.Deserialize<T>(dataJson, options);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Data özelliği çözümlenirken hata: {ex.Message}");
                                    }
                                }
                            }
                        }
                        catch (JsonException jex)
                        {
                            Console.WriteLine($"JSON yapısı kontrol edilirken hata oluştu: {jex.Message}");
                        }
                    }
                    
                    // Standart ayrıştırma
                    return JsonSerializer.Deserialize<T>(responseContent, options);
                }
                catch (JsonException jex)
                {
                    Console.WriteLine($"JSON parsing error: {jex.Message}");
                    Console.WriteLine($"Response content: {responseContent}");
                    
                    // Özel durum: Eğer $values içeren bir yanıt gelirse ve T bir liste ise
                    if (responseContent.Contains("$values") && typeof(T).IsGenericType && 
                        typeof(T).GetGenericTypeDefinition() == typeof(ApiResponse<>))
                    {
                        try
                        {
                            // JSON'ı manuel olarak ayrıştır
                            using (JsonDocument doc = JsonDocument.Parse(responseContent))
                            {
                                var root = doc.RootElement;
                                
                                // ApiResponse özelliklerini al
                                bool success = root.GetProperty("success").GetBoolean();
                                string message = root.GetProperty("message").GetString();
                                
                                // Data özelliğini al
                                var dataProperty = root.GetProperty("data");
                                
                                // Eğer data içinde $values varsa
                                if (dataProperty.TryGetProperty("$values", out var valuesProperty))
                                {
                                    // ApiResponse<T> oluştur
                                    var responseType = typeof(T);
                                    var dataType = responseType.GetGenericArguments()[0];
                                    
                                    // $values içeriğini doğrudan dataType'a dönüştür
                                    var valuesJson = valuesProperty.GetRawText();
                                    var dataValue = JsonSerializer.Deserialize(valuesJson, dataType, options);
                                    
                                    // ApiResponse<T> oluştur
                                    var apiResponse = Activator.CreateInstance(responseType);
                                    responseType.GetProperty("Success").SetValue(apiResponse, success);
                                    responseType.GetProperty("Message").SetValue(apiResponse, message);
                                    responseType.GetProperty("Data").SetValue(apiResponse, dataValue);
                                    
                                    return (T)apiResponse;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Özel JSON ayrıştırma hatası: {ex.Message}");
                        }
                    }
                    
                    throw new Exception($"JSON parsing error: {jex.Message} for content: {responseContent}", jex);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Error in GetAsync: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<T> PostAsync<T>(string endpoint, object data, string? token = null)
        {
            try
            {
                Console.WriteLine($"PostAsync başladı - Endpoint: {endpoint}, Token durumu: {(string.IsNullOrEmpty(token) ? "Boş" : "Var")}");
                
                // Authorization header'ı ekle
                AddAuthorizationHeader(token);
                
                var jsonContent = JsonSerializer.Serialize(data);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                
                Console.WriteLine($"İstek yapılıyor: {_httpClient.BaseAddress}{endpoint}");
                Console.WriteLine($"Gönderilen veri: {jsonContent}");
                
                var response = await _httpClient.PostAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API yanıtı - Status: {response.StatusCode}, Content (ilk 100 karakter): {responseContent.Substring(0, Math.Min(100, responseContent.Length))}...");
                
                if (!response.IsSuccessStatusCode)
                {
                    // 401 Unauthorized hatası için özel mesaj
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        Console.WriteLine("401 Unauthorized hatası alındı!");
                        Console.WriteLine($"Gönderilen token: {(string.IsNullOrEmpty(token) ? "BOŞ" : token.Substring(0, Math.Min(20, token.Length)) + "...")}");
                        throw new UnauthorizedAccessException("Bu işlemi gerçekleştirmek için yetkiniz bulunmamaktadır veya oturumunuz sona ermiştir.");
                    }
                    
                    Console.WriteLine($"API Error - Status: {response.StatusCode}, Content: {responseContent}");
                    throw new HttpRequestException($"API Error: {response.StatusCode} - {responseContent}");
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                try 
                {
                    var resultType = typeof(T);
                    
                    // Doğrudan bir JSON dizisi döndürüldüğünde işleyin (koleksiyon türleri için)
                    if (resultType.IsGenericType && 
                        (resultType.GetGenericTypeDefinition() == typeof(List<>) || 
                         resultType.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                         resultType.GetGenericTypeDefinition() == typeof(ICollection<>)))
                    {
                        try 
                        {
                            using (JsonDocument doc = JsonDocument.Parse(responseContent))
                            {
                                // Yanıt bir dizi mi?
                                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                                {
                                    Console.WriteLine("API bir dizi döndürdü, doğrudan ayrıştırılıyor");
                                    return JsonSerializer.Deserialize<T>(responseContent, options);
                                }
                            }
                        }
                        catch (JsonException jex)
                        {
                            Console.WriteLine($"JSON yapısı kontrol edilirken hata oluştu: {jex.Message}");
                        }
                    }
                    
                    // Eğer tip ApiResponse<something> değilse ve API yanıtı success/message/data formatındaysa
                    if (!resultType.IsGenericType || resultType.GetGenericTypeDefinition() != typeof(ApiResponse<>))
                    {
                        // ApiResponse<T> formatında olup olmadığını kontrol et
                        try
                        {
                            using (JsonDocument doc = JsonDocument.Parse(responseContent))
                            {
                                var root = doc.RootElement;
                                if (root.TryGetProperty("success", out _) && 
                                    root.TryGetProperty("data", out var dataProperty))
                                {
                                    // Eğer API bir ApiResponse döndürüyorsa ve biz sadece data'yı istiyorsak
                                    var dataJson = dataProperty.GetRawText();
                                    try
                                    {
                                        Console.WriteLine($"API bir ApiResponse döndürdü, data özelliği çözümleniyor: {dataJson}");
                                        return JsonSerializer.Deserialize<T>(dataJson, options);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Data özelliği çözümlenirken hata: {ex.Message}");
                                    }
                                }
                            }
                        }
                        catch (JsonException jex)
                        {
                            Console.WriteLine($"JSON yapısı kontrol edilirken hata oluştu: {jex.Message}");
                        }
                    }
                    
                    // Standart ayrıştırma
                    return JsonSerializer.Deserialize<T>(responseContent, options);
                }
                catch (JsonException jex)
                {
                    Console.WriteLine($"JSON parsing error: {jex.Message}");
                    Console.WriteLine($"Response content: {responseContent}");
                    
                    throw new Exception($"JSON parsing error: {jex.Message} for content: {responseContent}", jex);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Error in PostAsync: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                throw;
            }
            finally
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<T> PutAsync<T>(string endpoint, object data, string? token = null)
        {
            try
            {
                Console.WriteLine($"PutAsync başladı - Endpoint: {endpoint}, Token durumu: {(string.IsNullOrEmpty(token) ? "Boş" : "Var")}");
                
                // Authorization header'ı ekle
                AddAuthorizationHeader(token);
                
                var jsonContent = JsonSerializer.Serialize(data);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                
                Console.WriteLine($"İstek yapılıyor: {_httpClient.BaseAddress}{endpoint}");
                var response = await _httpClient.PutAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                {
                    // 401 Unauthorized hatası için özel mesaj
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        Console.WriteLine("401 Unauthorized hatası alındı!");
                        throw new UnauthorizedAccessException("Bu işlemi gerçekleştirmek için yetkiniz bulunmamaktadır veya oturumunuz sona ermiştir.");
                    }
                    
                    Console.WriteLine($"API Error - Status: {response.StatusCode}, Content: {responseContent}");
                    throw new HttpRequestException($"API Error: {response.StatusCode} - {responseContent}");
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                return JsonSerializer.Deserialize<T>(responseContent, options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Error in PutAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<T> DeleteAsync<T>(string endpoint, string? token = null)
        {
            try
            {
                Console.WriteLine($"DeleteAsync başladı - Endpoint: {endpoint}, Token durumu: {(string.IsNullOrEmpty(token) ? "Boş" : "Var")}");
                
                // Authorization header'ı ekle
                AddAuthorizationHeader(token);
                
                Console.WriteLine($"İstek yapılıyor: {_httpClient.BaseAddress}{endpoint}");
                var response = await _httpClient.DeleteAsync(endpoint);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                {
                    // 401 Unauthorized hatası için özel mesaj
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        Console.WriteLine("401 Unauthorized hatası alındı!");
                        throw new UnauthorizedAccessException("Bu işlemi gerçekleştirmek için yetkiniz bulunmamaktadır veya oturumunuz sona ermiştir.");
                    }
                    
                    Console.WriteLine($"API Error - Status: {response.StatusCode}, Content: {responseContent}");
                    throw new HttpRequestException($"API Error: {response.StatusCode} - {responseContent}");
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                return JsonSerializer.Deserialize<T>(responseContent, options);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Error in DeleteAsync: {ex.Message}");
                throw;
            }
        }

        // ... diğer metodlar
    }
} 