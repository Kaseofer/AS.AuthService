using AgendaSalud.AuthService.Application.DTOs;
using AgendaSalud.AuthService.Application.Interfaces;
using AgendaSalud.AuthService.Domain.Entities;
using System.Security.Cryptography;
using System.Text;

public class PasswordService : IPasswordService
{
    
    private readonly IGenericRepository<User> _userRepository;
    private readonly IGenericRepository<PasswordReset> _passwordResetRepository;

    
    public PasswordService(IGenericRepository<User> userRepository, IGenericRepository<PasswordReset> passwordResetRepository)
    {
        _userRepository = userRepository;
        _passwordResetRepository = passwordResetRepository;

    }


    public async Task<bool> RequestResetAsync(PasswordResetRequestDto dto)
    {
        try
        {
            var usuarios = await _userRepository.GetAllAsync();

            if (usuarios.Any())
            {
                throw new TaskCanceledException("RequestResetAsync: No se encontro el usuario");
            }

            var user = usuarios.FirstOrDefault(u => u.Email == dto.Email);

            if (user == null || !user.IsActive)
                throw new TaskCanceledException("RequestResetAsync: Usuario No encontrado o Inactivo");

            var token = GenerateSecureToken();
            var reset = new PasswordReset
            {
                UserId = user.Id,
                ResetToken = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30)
            };

            var rlt = await _passwordResetRepository.UpdateAsync(reset);

            if (!rlt)
            {
                throw new TaskCanceledException("RequestResetAsync: No se puedo registar reset password");
            }

            // Acá podrías enviar el token por email o loguearlo para pruebas
            
            Console.WriteLine($"Reset token for {user.Email}: {token}");

            return true;
        }
        catch
        {

            throw;
        }
        
    }

    public async Task<bool> ChangePasswordAsync(PasswordChangeDto dto)
    {
        try
        {
            
            var resets = await _passwordResetRepository.QueryAsync(r => r.ResetToken == dto.ResetToken && !r.Used, includeProperties: "User");
            if (resets.Any())
            {
                throw new TaskCanceledException("ChangePasswordAsync: No hay Reset de password");
            }

            var reset = resets.FirstOrDefault();

            if (reset == null || reset.ExpiresAt < DateTime.UtcNow)
                throw new TaskCanceledException("ChangePasswordAsync: Invalido o Expirado el Token");

            reset.User.PasswordHash = HashPassword(dto.NewPassword);
            reset.Used = true;

            var rst = await _passwordResetRepository.UpdateAsync(reset);

            return rst;

        }
        catch
        {
            throw;
        }
    }

    private string GenerateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes);
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
