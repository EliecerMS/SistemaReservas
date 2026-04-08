using Microsoft.Extensions.DependencyInjection;
using SistemaReservas.Application.Interfaces;
using SistemaReservas.Core.Interfaces;
using SistemaReservas.Infrastructure.Auth;
using SistemaReservas.Infrastructure.Repositories;

namespace SistemaReservas.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // authiorization services
            services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddSingleton<IPasswordHasher, PasswordHasher>();

            // rrepositories
            services.AddScoped<IUserRepository, DapperUserRepository>();
            services.AddScoped<IRefreshTokenRepository, DapperRefreshTokenRepository>();
            services.AddScoped<IZoneRepository, DapperZoneRepository>();
            services.AddScoped<IReservationRepository, DapperReservationRepository>();

            return services;
        }
    }
}
