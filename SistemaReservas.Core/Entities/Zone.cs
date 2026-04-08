namespace SistemaReservas.Core.Entities
{
    public class Zone
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public decimal PricePerHour { get; set; }
        public bool IsActive { get; set; } = true;

        // navigation
        public List<ZoneImage> Images { get; set; } = new();
        public List<EventType> EventTypes { get; set; } = new();
    }

    public class ZoneImage
    {
        public int Id { get; set; }
        public int ZoneId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }
}
