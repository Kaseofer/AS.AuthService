using AgendaSalud.AuthService.Domain.Entities;
using AgendaSalud.AuthService.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace AgendaSalud.AuthService.Infrastructure.Persistence.Seeders;

public static class RoleSeeder
{
    public static async Task SeedAsync(AuthenticationDbContext context)
    {
        var predefinedRoles = new[]
        {
            new Role { Name = "Admin", Description = "Administrador del Sistema" },
            new Role { Name = "Patient", Description = "Usuario que solicita turnos y servicios médicos" },
            new Role { Name = "Professional", Description = "Prestador de servicios médicos" },
            new Role { Name = "ScheduleManager", Description = "Administrador de agenda y disponibilidad" }
        };


        foreach (var role in predefinedRoles)
        {
            var exists = await context.Roles.AnyAsync(r => r.Name == role.Name);
            if (!exists)
            {
                await context.Roles.AddAsync(role);
            }
        }

        await context.SaveChangesAsync();
    }
}
