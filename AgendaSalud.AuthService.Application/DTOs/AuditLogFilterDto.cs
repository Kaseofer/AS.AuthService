namespace AgendaSalud.AuthService.Application.DTOs
{
    public class AuditLogFilterDto
    {
        public Guid? UserId { get; set; }
        public string? Action { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}
