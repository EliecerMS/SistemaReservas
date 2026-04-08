using SistemaReservas.Core.Entities;

namespace SistemaReservas.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user);
        /// <summary>Generates a cryptographically random opaque refresh token (64-char hex string).</summary>
        string GenerateRefreshToken();
    }

    public interface IPasswordHasher
    {
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
    }
}
