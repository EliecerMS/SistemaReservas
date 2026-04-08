using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaReservas.Application.DTOs;
using SistemaReservas.Application.Services;
using System.Security.Claims;

namespace SistemaReservas.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationsController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("No se pudo obtener el ID del usuario."));

        private bool IsAdmin() => User.IsInRole("Admin");


        // creates reservatiosn, requires a uthentication, returns 409 if the zone is already booked for the requested slot
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReservationDto dto)
        {
            try
            {
                var userId = GetUserId();
                var newId = await _reservationService.CreateAsync(userId, dto);
                return StatusCode(201, new { id = newId, message = "Reserva creada exitosamente." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                //  zone unavailable (conflict), capacity exceeded, zone inactive
                return Conflict(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return UnprocessableEntity(new { message = ex.Message });
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Message.Contains("ZONE_UNAVAILABLE"))
            {
                // concurrency conflict caught at DB level (by t he stored procedure)
                return Conflict(new { message = "La zona no está disponible en ese horario. Ya se reservó." });
            }
        }

        // cancels a reservation, users can only cancel their own under 48h rule, admins can cancel any

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var userId = GetUserId();
                await _reservationService.CancelAsync(userId, id, IsAdmin());
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // gets reservations for the current authenticated user
        [Authorize]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyReservations()
        {
            var userId = GetUserId();
            var reservations = await _reservationService.GetMyReservationsAsync(userId);
            return Ok(reservations);
        }


        // gets all reservations (Admin only), can filter by statusId

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? statusId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var reservations = await _reservationService.GetAllAsync(statusId, page, pageSize);
            return Ok(reservations);
        }


        // confirm a pending reservation (Admin only)

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/confirm")]
        public async Task<IActionResult> Confirm(int id)
        {
            try
            {
                await _reservationService.ConfirmAsync(id);
                return Ok(new { message = "Reserva confirmada exitosamente." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}
