namespace SistemaReservas.WebMVC.Models.ViewModels
{
    public class ZonesIndexViewModel
    {
        public required List<ApiDTOs.ZoneDto> Zones { get; init; } = [];
        public int CurrentPage { get; init; }
        public int TotalCount { get; init; }
        public int PageSize { get; init; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
