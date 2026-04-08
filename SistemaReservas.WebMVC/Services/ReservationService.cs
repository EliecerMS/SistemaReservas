using SistemaReservas.WebMVC.Models.ApiDTOs;
using System.Text.Json;

namespace SistemaReservas.WebMVC.Services
{
    public class ReservationService : BaseApiService, IReservationService
    {
        private readonly JsonSerializerOptions _jsonOptions;

        public ReservationService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor) : base(httpClient, httpContextAccessor)
        {
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<IEnumerable<ReservationDto>> GetMyReservationsAsync()
        {
            AddAuthCookiesToRequest();
            var response = await _httpClient.GetAsync("/api/reservations/my");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<ReservationDto>>(_jsonOptions) ?? Array.Empty<ReservationDto>();
        }

        public async Task<PagedResponseDto<ReservationDto>> GetAllAsync(int? statusId = null, int page = 1, int pageSize = 20)
        {
            AddAuthCookiesToRequest();
            var url = $"/api/reservations?page={page}&pageSize={pageSize}";
            if (statusId.HasValue) url += $"&statusId={statusId.Value}";

            var response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<PagedResponseDto<ReservationDto>>(_jsonOptions);

            return result ?? new PagedResponseDto<ReservationDto>
            {
                Items = [],
                TotalCount = 0,
                Page = page,
                PageSize = pageSize
            };


        }

        public async Task<int> CreateAsync(CreateReservationDto dto)
        {
            AddAuthCookiesToRequest();
            var response = await _httpClient.PostAsJsonAsync("/api/reservations", dto);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to create reservation: {error}");
            }

            var result = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
            return result.GetProperty("id").GetInt32();
        }

        public async Task CancelAsync(int id)
        {
            AddAuthCookiesToRequest();
            var response = await _httpClient.DeleteAsync($"/api/reservations/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task ConfirmAsync(int id)
        {
            AddAuthCookiesToRequest();
            var response = await _httpClient.PutAsync($"/api/reservations/{id}/confirm", null);
            response.EnsureSuccessStatusCode();
        }
    }
}
