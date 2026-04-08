namespace SistemaReservas.WebMVC.Models.ViewModels
{
    public class AdminIndexViewModel
    {

        public required List<ApiDTOs.ReservationDto> Reservations { get; init; } = [];
        public int CurrentPage { get; init; }
        public int TotalCount { get; init; }
        public int PageSize { get; init; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    }
}
