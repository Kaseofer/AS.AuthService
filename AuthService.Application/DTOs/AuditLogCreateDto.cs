namespace AgendaSalud.AuthService.Application.DTOs
{
    public class AuditLogCreateDto
    {
        public Guid UserId { get; set; }
        public string Action { get; set; }
        public string Endpoint { get; set; }
        public string IpAddress { get; set; }
    }

}
