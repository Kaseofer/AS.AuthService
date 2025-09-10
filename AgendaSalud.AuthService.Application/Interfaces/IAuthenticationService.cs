using AgendaSalud.AuthService.Application.DTOs;

namespace AgendaSalud.AuthService.Application.Interfaces
{
    public interface IAuthenticationService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterUserDto dto);
        Task<AuthResponseDto> LoginAsync(LoginUserDto dto);
    }
}
