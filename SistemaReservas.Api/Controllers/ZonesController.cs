using Microsoft.AspNetCore.Mvc;
using SistemaReservas.Application.Services;

namespace SistemaReservas.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ZonesController : ControllerBase
    {
        private readonly IZoneService _zoneService;

        public ZonesController(IZoneService zoneService)
        {
            _zoneService = zoneService;
        }


        // lists all active zones with their images and supported event types, public endpoint

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var zones = await _zoneService.GetAllAsync(page, pageSize);
            return Ok(zones);
        }


        // gets details of a specific zone, public endpoin t
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var zone = await _zoneService.GetByIdAsync(id);
            if (zone == null)
                return NotFound(new { message = "Zona no encontrada." });
            return Ok(zone);
        }

        // lists all event types, public endpoint
        [HttpGet("event-types")]
        public async Task<IActionResult> GetEventTypes()
        {
            var eventTypes = await _zoneService.GetAllEventTypesAsync();
            return Ok(eventTypes);
        }
    }
}
