namespace AgendaSalud.AuthService.Application.DTOs
{
    public class TokenValidationDto
    {
        public bool IsValid { get; set; }
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
    }
}