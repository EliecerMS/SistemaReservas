using Microsoft.AspNetCore.Mvc;
using SistemaReservas.Application.DTOs;
using SistemaReservas.Application.Services;

namespace SistemaReservas.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                await _authService.RegisterAsync(dto);
                return StatusCode(201, new { message = "Usuario registrado exitosamente." });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var result = await _authService.LoginAsync(dto);
                SetAuthCookies(result);

                return Ok(new
                {
                    fullName = result.FullName,
                    email    = result.Email,
                    role     = result.Role
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies["refresh_token"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized(new { message = "Token de refresco no encontrado." });

            try
            {
                var result = await _authService.RefreshAsync(refreshToken);
                SetAuthCookies(result);

                return Ok(new
                {
                    fullName = result.FullName,
                    email    = result.Email,
                    role     = result.Role
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                // Clear stale cookies so the frontend knows the session is dead
                DeleteAuthCookies();
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refresh_token"];
            if (!string.IsNullOrEmpty(refreshToken))
                await _authService.RevokeRefreshTokenAsync(refreshToken);

            DeleteAuthCookies();
            return Ok(new { message = "Sesión cerrada exitosamente." });
        }

        // ── private helpers ──────────────────────────────────────────────

        private void SetAuthCookies(LoginResponseDto result)
        {
            var accessExpiry   = int.Parse(_configuration["JwtSettings:ExpiryMinutes"]!);
            var refreshExpiry  = int.Parse(_configuration["JwtSettings:RefreshTokenExpiryDays"] ?? "7");

            var baseOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure   = true,
                SameSite = SameSiteMode.None
            };

            Response.Cookies.Append("auth_token", result.Token, new CookieOptions
            {
                HttpOnly = baseOptions.HttpOnly,
                Secure   = baseOptions.Secure,
                SameSite = baseOptions.SameSite,
                Expires  = DateTime.UtcNow.AddMinutes(accessExpiry)
            });

            Response.Cookies.Append("refresh_token", result.RefreshToken, new CookieOptions
            {
                HttpOnly = baseOptions.HttpOnly,
                Secure   = baseOptions.Secure,
                SameSite = baseOptions.SameSite,
                Expires  = DateTime.UtcNow.AddDays(refreshExpiry),
                Path     = "/api/auth/refresh" // scope the cookie to the refresh endpoint only
            });
        }

        private void DeleteAuthCookies()
        {
            var opts = new CookieOptions
            {
                HttpOnly = true,
                Secure   = true,
                SameSite = SameSiteMode.None
            };
            Response.Cookies.Delete("auth_token", opts);
            Response.Cookies.Delete("refresh_token", new CookieOptions
            {
                HttpOnly = opts.HttpOnly,
                Secure   = opts.Secure,
                SameSite = opts.SameSite,
                Path     = "/api/auth/refresh"
            });
        }
    }
}
