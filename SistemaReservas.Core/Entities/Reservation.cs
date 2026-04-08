namespace SistemaReservas.Core.Entities
{
    public class Reservation
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ZoneId { get; set; }
        public int EventTypeId { get; set; }
        public int StatusId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int GuestCount { get; set; }
        public decimal TotalPrice { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // navigation properties
        public string ZoneName { get; set; } = string.Empty;
        public string EventTypeName { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
    }

    public class ReservationStatus
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
