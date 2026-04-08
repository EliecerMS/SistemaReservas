using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaReservas.WebMVC.Models.ViewModels;
using SistemaReservas.WebMVC.Services;
using System.Text.Json;

namespace SistemaReservas.WebMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IReservationService _reservationService;

        public AdminController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? statusId = null, int page = 1)
        {
            // 1. Obtener los datos de la API (Esto devuelve PagedResponseDto<ReservationDto>)
            var pagedData = await _reservationService.GetAllAsync(statusId, page, 20);
            ViewData["CurrentStatus"] = statusId;




            // 2. Mapear MANUALMENTE al ViewModel que requiere la Vista
            var viewModel = new AdminIndexViewModel
            {
                // Convertir en IEnumerable a List para ViewModel
                Reservations = pagedData.Items.ToList(),

                // Pasar metadatos de paginación
                CurrentPage = pagedData.Page,
                TotalCount = pagedData.TotalCount,
                PageSize = pagedData.PageSize
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            try
            {
                await _reservationService.ConfirmAsync(id);
                TempData["Notification"] = JsonSerializer.Serialize(new
                {
                    icon = "success",
                    title = "Confirmada",
                    text = "Reserva confirmada exitosamente.",
                    timer = 2000
                });
            }
            catch (Exception ex)
            {
                TempData["Notification"] = JsonSerializer.Serialize(new
                {
                    icon = "error",
                    title = "Error",
                    text = "No se pudo confirmar la reserva.",
                    timer = 3000
                });
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
