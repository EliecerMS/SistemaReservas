using System.Text.Json.Serialization;

namespace SistemaReservas.WebMVC.Models.ApiDTOs
{
    public class ReservationDto
    {
        public int Id { get; set; }
        public int ZoneId { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public int UserId { get; set; }
        
        [JsonPropertyName("userName")]
        public string? UserName { get; set; } // Depending on API implementation, could be populated for admins
        
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Attendees { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public int EventTypeId { get; set; }
        public string EventTypeName { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class CreateReservationDto
    {
        public int ZoneId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Attendees { get; set; }
        public int EventTypeId { get; set; }
        public string? Notes { get; set; }
    }
}
