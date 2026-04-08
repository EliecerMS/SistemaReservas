using Microsoft.AspNetCore.Mvc;
using SistemaReservas.WebMVC.Models.ViewModels;
using SistemaReservas.WebMVC.Services;

namespace SistemaReservas.WebMVC.Controllers
{
    public class ZonesController : Controller
    {
        private readonly IZoneService _zoneService;

        public ZonesController(IZoneService zoneService)
        {
            _zoneService = zoneService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, int pageSize = 20)
        {
            // 1. Obtener los datos de la API (Esto devuelve PagedResponseDto<ZoneDto>)
            var pagedData = await _zoneService.GetAllAsync(page, pageSize);

            // 2. Mapear MANUALMENTE al ViewModel que requiere la Vista
            var viewModel = new ZonesIndexViewModel
            {
                // convertimos el IEnumerable a List para el ViewModel
                Zones = pagedData.Items.ToList(),

                // pasar  metadatos de paginación
                CurrentPage = pagedData.Page,
                TotalCount = pagedData.TotalCount,
                PageSize = pagedData.PageSize
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var zone = await _zoneService.GetByIdAsync(id);
            if (zone == null) return NotFound();
            return View(zone);
        }
    }
}
