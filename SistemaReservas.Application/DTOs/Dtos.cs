using System.ComponentModel.DataAnnotations;

namespace SistemaReservas.Application.DTOs
{

    public class LoginDto
    {
        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterDto
    {
        [Required(ErrorMessage = "El nombre completo es requerido")]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es requerido")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class ZoneResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public decimal PricePerHour { get; set; }
        public bool IsActive { get; set; }
        public List<ZoneImageDto> Images { get; set; } = new();
        public List<EventTypeDto> EventTypes { get; set; } = new();
    }

    public class ZoneImageDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }

    public class EventTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class CreateReservationDto
    {
        [Required(ErrorMessage = "La zona es requerida")]
        public int ZoneId { get; set; }

        [Required(ErrorMessage = "El tipo de evento es requerido")]
        public int EventTypeId { get; set; }

        [Required(ErrorMessage = "La fecha y hora de inicio es requerida")]
        public DateTime StartDateTime { get; set; }

        [Required(ErrorMessage = "La fecha y hora de fin es requerida")]
        public DateTime EndDateTime { get; set; }

        [Required(ErrorMessage = "El número de invitados es requerido")]
        [Range(1, 10000, ErrorMessage = "El número de invitados debe ser al menos 1")]
        public int GuestCount { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;
    }

    public class ReservationResponseDto
    {
        public int Id { get; set; }
        public string ZoneName { get; set; } = string.Empty;
        public string EventTypeName { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int GuestCount { get; set; }
        public decimal TotalPrice { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string UserFullName { get; set; } = string.Empty;
    }


    public class AvailabilityRequestDto
    {
        [Required]
        public int ZoneId { get; set; }
        [Required]
        public DateTime Date { get; set; }
    }
}
