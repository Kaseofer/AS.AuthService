using AgendaSalud.AuthService.Domain.Entities;

namespace AgendaSalud.AuthService.Application.Interfaces
{
    public interface IJwtGeneratorService
    {
        string GenerateToken(User user);
        Guid? ValidateTokenAndGetUserId(string token);
    }
}