using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaReservas.WebMVC.Models.ApiDTOs;
using SistemaReservas.WebMVC.Services;
using System.Text.Json;

namespace SistemaReservas.WebMVC.Controllers
{
    [Authorize]
    public class ReservationsController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly IZoneService _zoneService;

        public ReservationsController(IReservationService reservationService, IZoneService zoneService)
        {
            _reservationService = reservationService;
            _zoneService = zoneService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var reservations = await _reservationService.GetMyReservationsAsync();
            return View(reservations);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int id)
        {
            var zone = await _zoneService.GetByIdAsync(id);
            if (zone == null) return NotFound();

            var eventTypes = await _zoneService.GetEventTypesAsync();
            ViewBag.EventTypes = new SelectList(eventTypes, "Id", "Name");

            var dto = new CreateReservationDto 
            { 
                ZoneId = id, 
                StartTime = DateTime.Now.AddDays(1).Date.AddHours(9), // default 9 am tomorrow
                EndTime = DateTime.Now.AddDays(1).Date.AddHours(11), 
                Attendees = 1 
            };

            ViewBag.Zone = zone;
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReservationDto dto)
        {
            if (!ModelState.IsValid)
            {
                var eventTypes = await _zoneService.GetEventTypesAsync();
                ViewBag.EventTypes = new SelectList(eventTypes, "Id", "Name");
                ViewBag.Zone = await _zoneService.GetByIdAsync(dto.ZoneId);
                return View(dto);
            }

            try
            {
                await _reservationService.CreateAsync(dto);
                
                TempData["Notification"] = JsonSerializer.Serialize(new 
                { 
                    icon = "success", 
                    title = "¡Reserva Creada!", 
                    text = "Tu reserva se ha procesado exitosamente.", 
                    timer = 3000 
                });

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var eventTypes = await _zoneService.GetEventTypesAsync();
                ViewBag.EventTypes = new SelectList(eventTypes, "Id", "Name");
                ViewBag.Zone = await _zoneService.GetByIdAsync(dto.ZoneId);
                
                // Map the error string thrown
                ModelState.AddModelError(string.Empty, "Hubo un problema procesando tu reserva (ej. choque de horario o capacidad).");
                return View(dto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                await _reservationService.CancelAsync(id);
                TempData["Notification"] = JsonSerializer.Serialize(new 
                { 
                    icon = "success", 
                    title = "Cancelada", 
                    text = "La reserva fue cancelada exitosamente.", 
                    timer = 2000 
                });
            }
            catch (Exception)
            {
                TempData["Notification"] = JsonSerializer.Serialize(new 
                { 
                    icon = "error", 
                    title = "Error", 
                    text = "No se pudo cancelar (puede que sea menor a 48hs).", 
                    timer = 3000 
                });
            }

            // Redirect back to either My Reservations or Admin dashboard based on role
            if (User.IsInRole("Admin") && Request.Headers["Referer"].ToString().Contains("/Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
