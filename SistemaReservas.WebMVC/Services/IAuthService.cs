using SistemaReservas.WebMVC.Models.ApiDTOs;

namespace SistemaReservas.WebMVC.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginDto dto);
        Task RegisterAsync(RegisterDto dto);
        Task LogoutAsync();
    }
}
