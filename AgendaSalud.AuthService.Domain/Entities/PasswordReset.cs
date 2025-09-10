namespace AgendaSalud.AuthService.Domain.Entities
{
    public class PasswordReset
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string ResetToken { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
        public bool Used { get; set; } = false;
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User User { get; set; } = null!;
    }
}
