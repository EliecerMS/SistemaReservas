using SistemaReservas.WebMVC.Models.ApiDTOs;

namespace SistemaReservas.WebMVC.Services
{
    public interface IZoneService
    {
        Task<PagedResponseDto<ZoneDto>> GetAllAsync(int page = 1, int pageSize = 20);
        Task<ZoneDto?> GetByIdAsync(int id);
        Task<IEnumerable<EventTypeDto>> GetEventTypesAsync();
    }
}
