namespace AgendaSalud.AuthService.Application.DTOs
{
    public class ExternalLoginDto
    {
        public string Provider { get; set; } // "Google", "Facebook", etc.
        public string ProviderId { get; set; } // ID del usuario en el provider externo (antes era ExternalId)
        public string Email { get; set; }
        public string FullName { get; set; }
    }

}
