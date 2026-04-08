namespace SistemaReservas.WebMVC.Models.ApiDTOs
{
    public class LoginResponseDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        // Token and RefreshToken are usually set in cookie by the API and might not be serialized to the client bodies directly
        // but AuthController return Ok(new { fullName, email, role }), so we return those
    }
}
