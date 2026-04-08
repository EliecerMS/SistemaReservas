using Microsoft.Extensions.DependencyInjection;
using SistemaReservas.Application.Services;

namespace SistemaReservas.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IZoneService, ZoneService>();
            services.AddScoped<IReservationService, ReservationService>();
            return services;
        }
    }
}
