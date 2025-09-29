using AgendaSalud.AuthService.Domain.Entities;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgendaSalud.AuthService.Infrastructure.Persistence.Context;

public class AuthenticationDbContext : DbContext
{
    public AuthenticationDbContext(DbContextOptions<AuthenticationDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<PasswordReset> PasswordResets => Set<PasswordReset>();
    public DbSet<ExternalLogin> ExternalLogins => Set<ExternalLogin>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>().ToTable("user", "security");
        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId);

        modelBuilder.Entity<User>()
            .HasMany(u => u.AuditLogs)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserId);

        modelBuilder.Entity<User>()
            .HasMany(u => u.ExternalLogins)
            .WithOne(e => e.User)
            .HasForeignKey(e => e.UserId);

        // Role
        modelBuilder.Entity<Role>().ToTable("role", "security");
        modelBuilder.Entity<Role>()
            .HasIndex(r => r.Name)
            .IsUnique();

        // AuditLog
        modelBuilder.Entity<AuditLog>().ToTable("audit_log", "security");
        modelBuilder.Entity<AuditLog>()
            .Property(a => a.Timestamp)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // PasswordReset → corregido: tabla propia
        modelBuilder.Entity<PasswordReset>().ToTable("password_reset", "security");
        modelBuilder.Entity<PasswordReset>()
            .Property(p => p.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        // ExternalLogin → corregido: typo
        modelBuilder.Entity<ExternalLogin>().ToTable("external_login", "security");
        modelBuilder.Entity<ExternalLogin>()
            .Property(e => e.LinkedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
    }

}
