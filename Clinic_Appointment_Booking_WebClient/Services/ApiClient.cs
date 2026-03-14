using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Clinic_Appointment_Booking_WebClient.Services
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ApiClient> _logger;

        public ApiClient(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ApiClient> logger)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private void AddAuthorizationHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("AccessToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<ApiResponse<T>?> GetAsync<T>(string endpoint)
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.GetAsync(endpoint);
                return await ProcessResponse<T>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"GET request failed: {ex.Message}");
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = "An error occurred while communicating with the server",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<T>?> PostAsync<T>(string endpoint, object? data = null)
        {
            try
            {
                AddAuthorizationHeader();
                var content = data != null
                    ? new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json")
                    : null;

                var response = await _httpClient.PostAsync(endpoint, content);
                return await ProcessResponse<T>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"POST request failed: {ex.Message}");
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = "An error occurred while communicating with the server",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<T>?> PutAsync<T>(string endpoint, object data)
        {
            try
            {
                AddAuthorizationHeader();
                var content = new StringContent(
                    JsonSerializer.Serialize(data),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PutAsync(endpoint, content);
                return await ProcessResponse<T>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"PUT request failed: {ex.Message}");
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = "An error occurred while communicating with the server",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ApiResponse<T>?> DeleteAsync<T>(string endpoint)
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.DeleteAsync(endpoint);
                return await ProcessResponse<T>(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"DELETE request failed: {ex.Message}");
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = "An error occurred while communicating with the server",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        private async Task<ApiResponse<T>?> ProcessResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(content))
            {
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = "Empty response from server"
                };
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            try
            {
                return JsonSerializer.Deserialize<ApiResponse<T>>(content, options);
            }
            catch (JsonException ex)
            {
                _logger.LogError($"Failed to deserialize API response: {ex.Message}. Content: {content}");

                return new ApiResponse<T>
                {
                    Success = false,
                    Message = "Unexpected response format from server",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}
