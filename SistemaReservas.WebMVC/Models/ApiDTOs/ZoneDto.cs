namespace SistemaReservas.WebMVC.Models.ApiDTOs
{
    public class ZoneDto
    {
        public int Id { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public int Capacity { get; init; }
        public decimal PricePerHour { get; init; }
        public bool IsActive { get; init; }
        public List<ZoneImageDto> Images { get; init; } = [];
        public List<EventTypeDto> EventTypes { get; init; } = [];
    }

    public record ZoneImageDto(int Id, string ImageUrl, bool IsPrimary);
    public record EventTypeDto(int Id, string Name, string Description);

}
