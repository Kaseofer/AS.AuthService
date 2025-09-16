using AgendaSalud.AuthService.Domain.Entities;
using System.Security.Claims;

namespace AgendaSalud.AuthService.Application.Interfaces
{
    public interface IJwtGeneratorService
    {
        string GenerateToken(User user);
        Guid? ValidateTokenAndGetUserId(string token);

        ClaimsPrincipal ValidateToken(string token); // Agregar este método

    }
}