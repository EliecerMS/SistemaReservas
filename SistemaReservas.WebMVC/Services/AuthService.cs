using Microsoft.AspNetCore.Http;
using SistemaReservas.WebMVC.Models.ApiDTOs;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SistemaReservas.WebMVC.Services
{
    public class AuthService : BaseApiService, IAuthService
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public AuthService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
            : base(httpClient, httpContextAccessor)
        {
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", dto);
            
            if (response.IsSuccessStatusCode)
            {
                ProxySetCookieHeaders(response);
                return await response.Content.ReadFromJsonAsync<LoginResponseDto>(_jsonOptions);
            }

            // In a real scenario, map error properly
            throw new Exception("Login failed");
        }

        public async Task RegisterAsync(RegisterDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/auth/register", dto);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Registration failed");
            }
        }

        public async Task LogoutAsync()
        {
            AddAuthCookiesToRequest();
            var response = await _httpClient.PostAsync("/api/auth/logout", null);
            ProxySetCookieHeaders(response); 
        }

        private void ProxySetCookieHeaders(HttpResponseMessage response)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null && response.Headers.TryGetValues("Set-Cookie", out var setCookieValues))
            {
                foreach (var cookieValue in setCookieValues)
                {
                    context.Response.Headers.Append("Set-Cookie", cookieValue);
                }
            }
        }
    }
}
