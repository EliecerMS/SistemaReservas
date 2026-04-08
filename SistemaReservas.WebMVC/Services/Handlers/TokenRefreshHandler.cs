using Microsoft.AspNetCore.Http;
using System.Net;

namespace SistemaReservas.WebMVC.Services.Handlers
{
    public class TokenRefreshHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;

        public TokenRefreshHandler(IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Proceed with the original request
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Attempt to refresh token
                var context = _httpContextAccessor.HttpContext;
                var refreshToken = context?.Request.Cookies["refresh_token"];
                
                if (string.IsNullOrEmpty(refreshToken))
                {
                    return response; // Cannot refresh without refresh token
                }

                // Create a temporary client to call refresh endpoint
                var refreshClient = _httpClientFactory.CreateClient("AuthRefreshClient");
                
                // Add the existing refresh token to the cookie
                refreshClient.DefaultRequestHeaders.Add("Cookie", $"refresh_token={refreshToken}");
                
                var refreshResponse = await refreshClient.PostAsync("/api/auth/refresh", null, cancellationToken);
                
                if (refreshResponse.IsSuccessStatusCode)
                {
                    // Proxy Set-Cookie headers from API to MVC Response
                    if (refreshResponse.Headers.TryGetValues("Set-Cookie", out var setCookieValues) && context != null)
                    {
                        foreach (var cookieValue in setCookieValues)
                        {
                            context.Response.Headers.Append("Set-Cookie", cookieValue);
                        }
                    }

                    // Retry original request by cloning it since RequestMessage can only be sent once
                    var clonedRequest = await CloneRequestAsync(request);
                    
                    // Update cloned request with new cookies
                    var newAuthToken = string.Empty;
                    var newRefreshToken = string.Empty;
                    
                    // We need to parse new auth_token from set-cookie header
                    if (setCookieValues != null)
                    {
                        foreach (var val in setCookieValues)
                        {
                            if (val.StartsWith("auth_token="))
                                newAuthToken = val.Split(';')[0].Substring("auth_token=".Length);
                            else if (val.StartsWith("refresh_token="))
                                newRefreshToken = val.Split(';')[0].Substring("refresh_token=".Length);
                        }
                    }

                    var cookieList = new List<string>();
                    if (!string.IsNullOrEmpty(newAuthToken)) cookieList.Add($"auth_token={newAuthToken}");
                    if (!string.IsNullOrEmpty(newRefreshToken)) cookieList.Add($"refresh_token={newRefreshToken}");
                    else cookieList.Add($"refresh_token={refreshToken}"); // if refresh token wasn't rotated

                    clonedRequest.Headers.Remove("Cookie");
                    if (cookieList.Count > 0)
                    {
                        clonedRequest.Headers.Add("Cookie", string.Join("; ", cookieList));
                    }

                    response = await base.SendAsync(clonedRequest, cancellationToken);
                }
            }

            return response;
        }

        private async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage req)
        {
            var clone = new HttpRequestMessage(req.Method, req.RequestUri);

            // Copy content
            if (req.Content != null)
            {
                var memoryStream = new MemoryStream();
                await req.Content.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                clone.Content = new StreamContent(memoryStream);

                if (req.Content.Headers != null)
                {
                    foreach (var h in req.Content.Headers)
                    {
                        clone.Content.Headers.Add(h.Key, h.Value);
                    }
                }
            }

            // Copy headers
            foreach (var header in req.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            // Copy properties (version etc)
            clone.Version = req.Version;

            return clone;
        }
    }
}
