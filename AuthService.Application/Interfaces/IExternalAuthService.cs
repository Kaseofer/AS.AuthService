using AgendaSalud.AuthService.Application.DTOs;

namespace AgendaSalud.AuthService.Application.Interfaces
{
    public interface IExternalAuthService
    {
        Task<AuthResponseDto> LoginOrRegisterAsync(ExternalLoginDto dto);
    }
}
