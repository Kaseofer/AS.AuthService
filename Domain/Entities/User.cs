using Domain.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgendaSalud.AuthService.Domain.Entities
{
    [Table("user", Schema = "security")]
    public class User
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("email")]
        public string Email { get; set; }

        [Required]
        [Column("password_hash")]
        public string PasswordHash { get; set; }

        [MaxLength(255)]
        [Column("full_name")]
        public string FullName { get; set; }

        [Required]
        [Column("role_id")]
        public Guid RoleId { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("failed_attempts")]
        public int FailedAttempts { get; set; } = 0;

        [Column("is_locked")]
        public bool IsLocked { get; set; } = false;

        [Column("last_failed_login")]
        public DateTime? LastFailedLogin { get; set; }

        [Column("force_password_change")]
        public bool ForcePasswordChange { get; set; } = false;

        // Nota: En tu DB tiene un espacio "next_allowed_login " - deberías corregirlo
        [Column("next_allowed_login ")]
        public DateTime? NextAllowedLogin { get; set; }

        [Column("last_successful_login")]
        public DateTime? LastSuccessfulLogin { get; set; }

        [Column("password_changed_at")]
        public DateTime? PasswordChangedAt { get; set; }

        // Navegación
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; }

        public virtual ICollection<ExternalLogin> ExternalLogins { get; set; }

        public virtual ICollection<AuditLog> AuditLogs { get; set; }
        
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
