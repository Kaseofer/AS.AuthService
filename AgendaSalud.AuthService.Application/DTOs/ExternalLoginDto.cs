namespace AgendaSalud.AuthService.Application.DTOs
{
    public class ExternalLoginDto
    {
        public string Provider { get; set; } = null!; // Ej: 'Google'
        public string ExternalId { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
    }

}
