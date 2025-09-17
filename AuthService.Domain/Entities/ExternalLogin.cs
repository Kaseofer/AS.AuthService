namespace AgendaSalud.AuthService.Domain.Entities
{
    public class ExternalLogin
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Provider { get; set; } = null!; // 'Google', 'Facebook'
        public string ExternalId { get; set; } = null!;
        public DateTime LinkedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User User { get; set; } = null!;

    }
}