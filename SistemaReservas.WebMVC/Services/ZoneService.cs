using SistemaReservas.WebMVC.Models.ApiDTOs;
using System.Text.Json;

namespace SistemaReservas.WebMVC.Services
{
    public class ZoneService : BaseApiService, IZoneService
    {
        private readonly JsonSerializerOptions _jsonOptions;

        public ZoneService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor) : base(httpClient, httpContextAccessor)
        {
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<PagedResponseDto<ZoneDto>> GetAllAsync(int page = 1, int pageSize = 20)
        {
            // 1. Construcción de la URL usando el prefijo configurado en el BaseAddress
            var url = $"zones?page={page}&pageSize={pageSize}";

            // 2. Ejecución de la petición asíncrona
            var response = await _httpClient.GetAsync(url);

            // 3. Validación de éxito (Lanza excepción si no es 2xx)
            response.EnsureSuccessStatusCode();

            // 4. Deserialización usando las opciones configuradas en la base (CamelCase)
            var result = await response.Content.ReadFromJsonAsync<PagedResponseDto<ZoneDto>>(_jsonOptions);

            // 5. Manejo de nulidad para cumplir con miembros 'required' de C# 13
            return result ?? new PagedResponseDto<ZoneDto>
            {
                Items = [],
                TotalCount = 0,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ZoneDto?> GetByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"/api/zones/{id}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ZoneDto>(_jsonOptions);
            }
            return null;
        }

        public async Task<IEnumerable<EventTypeDto>> GetEventTypesAsync()
        {
            var response = await _httpClient.GetAsync("/api/zones/event-types");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<EventTypeDto>>(_jsonOptions) ?? Array.Empty<EventTypeDto>();
        }
    }
}
