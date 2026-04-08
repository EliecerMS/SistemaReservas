using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace SistemaReservas.WebMVC.Services
{
    public abstract class BaseApiService
    {
        protected readonly HttpClient _httpClient;
        protected readonly IHttpContextAccessor _httpContextAccessor;

        protected BaseApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        protected void AddAuthCookiesToRequest()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context != null)
            {
                var authToken = context.Request.Cookies["auth_token"];
                var refreshToken = context.Request.Cookies["refresh_token"];
                var cookieHeader = new List<string>();

                if (!string.IsNullOrEmpty(authToken))
                    cookieHeader.Add($"auth_token={authToken}");

                if (!string.IsNullOrEmpty(refreshToken))
                    cookieHeader.Add($"refresh_token={refreshToken}");

                if (cookieHeader.Count > 0)
                {
                    _httpClient.DefaultRequestHeaders.Add("Cookie", string.Join("; ", cookieHeader));
                }
            }
        }
    }
}
