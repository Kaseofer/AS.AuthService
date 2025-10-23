using AgendaSalud.AuthService.Application.DTOs;
using AgendaSalud.AuthService.Application.Interfaces;
using AgendaSalud.AuthService.Domain.Entities;
using System.Security.Cryptography;
using System.Text;

public class PasswordService : IPasswordService
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IGenericRepository<PasswordReset> _passwordResetRepository;

    public PasswordService(
        IGenericRepository<User> userRepository,
        IGenericRepository<PasswordReset> passwordResetRepository)
    {
        _userRepository = userRepository;
        _passwordResetRepository = passwordResetRepository;
    }

    public async Task<bool> RequestResetAsync(PasswordResetRequestDto dto)
    {
        try
        {
            var users = await _userRepository.QueryAsync(u => u.Email == dto.Email);

            if (!users.Any())
            {
                // NO lanzar excepción para evitar enumeration attacks
                // Simplemente retornar sin enviar email
                return true;
            }

            var user = users.First();

            if (!user.IsActive)
            {
                return true; // Tampoco revelar que existe pero está inactivo
            }

            // Invalidar tokens anteriores
            var oldResets = await _passwordResetRepository.QueryAsync(
                r => r.UserId == user.Id && !r.Used && r.ExpiresAt > DateTime.UtcNow);

            foreach (var oldReset in oldResets)
            {
                oldReset.Used = true;
                await _passwordResetRepository.UpdateAsync(oldReset);
            }

            var token = GenerateSecureToken();

            var reset = new PasswordReset
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                ResetToken = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                Used = false,
                CreatedAt = DateTime.UtcNow
            };

            await _passwordResetRepository.AddAsync(reset);


            // Enviar email con link al FRONTEND
            var resetUrl = $"https://app.agendasalud.com/reset-password?token={token}";

            // TODO: Implementar envío de email
            // await _emailService.SendPasswordResetEmail(user.Email, resetUrl, user.FullName);

            Console.WriteLine($"🔑 Reset URL for {user.Email}:");
            Console.WriteLine($"   {resetUrl}");
            Console.WriteLine($"⏰ Expires at: {reset.ExpiresAt}");

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in RequestResetAsync: {ex.Message}");
            return true; // No revelar errores
        }
    }
    public async Task<bool> ChangePasswordAsync(PasswordChangeDto dto)
    {
        try
        {
            var resets = await _passwordResetRepository.QueryAsync(
                r => r.ResetToken == dto.ResetToken && !r.Used,
                includeProperties: "User");

            // ✅ CORREGIDO: Lógica invertida - debería ser !Any()
            if (!resets.Any())
            {
                throw new Exception("Token de reset inválido o ya usado");
            }

            var reset = resets.First();

            // Verificar expiración
            if (reset.ExpiresAt < DateTime.UtcNow)
            {
                throw new Exception("El token ha expirado");
            }

            // Verificar que el usuario existe y está activo
            if (reset.User == null || !reset.User.IsActive)
            {
                throw new Exception("Usuario no encontrado o inactivo");
            }

            // Cambiar contraseña
            reset.User.PasswordHash = HashPassword(dto.NewPassword);
            reset.User.PasswordChangedAt = DateTime.UtcNow;
            reset.User.UpdatedAt = DateTime.UtcNow;
            reset.User.ForcePasswordChange = false; // Ya cambió la contraseña

            // ✅ CORREGIDO: Actualizar usuario también
            await _userRepository.UpdateAsync(reset.User);

            // Marcar token como usado
            reset.Used = true;
            await _passwordResetRepository.UpdateAsync(reset);

            return true;
        }
        catch
        {
            throw;
        }
    }

    private string GenerateSecureToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    private string HashPassword(string password)
    {
        // TODO: Cambiar a BCrypt en producción
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}