using SistemaReservas.WebMVC.Models.ApiDTOs;

namespace SistemaReservas.WebMVC.Services
{
    public interface IReservationService
    {
        Task<IEnumerable<ReservationDto>> GetMyReservationsAsync();
        Task<PagedResponseDto<ReservationDto>> GetAllAsync(int? statusId = null, int page = 1, int pageSize = 20);
        Task<int> CreateAsync(CreateReservationDto dto);
        Task CancelAsync(int id);
        Task ConfirmAsync(int id);
    }
}
