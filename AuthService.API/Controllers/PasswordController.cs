using AgendaSalud.AuthService.Api.Common;
using AgendaSalud.AuthService.Application.DTOs;
using AgendaSalud.AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("password")]
public class PasswordController : ControllerBase
{
    private readonly IPasswordService _passwordService;

    public PasswordController(IPasswordService passwordService)
    {
        _passwordService = passwordService;
    }

    [HttpPost("request-reset")]
    [AllowAnonymous] // ← Importante: debe ser público
    public async Task<IActionResult> RequestReset([FromBody] PasswordResetRequestDto dto)
    {
        var response = new ResponseApi<bool>();
        try
        {
            // Validación del email
            if (string.IsNullOrEmpty(dto.Email) || !IsValidEmail(dto.Email))
            {
                response.IsSuccess = false;
                response.Message = "Email inválido";
                response.ErrorCode = "INVALID_EMAIL";
                return BadRequest(response);
            }

            await _passwordService.RequestResetAsync(dto);

            // ⚠️ IMPORTANTE: Siempre responder éxito para evitar enumeration attacks
            // No revelar si el email existe o no

            response.IsSuccess = true;
            response.Message = "Si el email existe, recibirás un enlace para restablecer tu contraseña";
            response.Data = true;

            return Ok(response);
        }
        catch (Exception ex)
        {
            // Loguear el error pero NO exponerlo al usuario
            Console.WriteLine($"Error in RequestReset: {ex.Message}");

            // Responder siempre igual por seguridad
            response.IsSuccess = true;
            response.Message = "Si el email existe, recibirás un enlace para restablecer tu contraseña";
            response.Data = true;

            return Ok(response);
        }
    }

    [HttpPost("change")]
    [AllowAnonymous] // ← Usuario no está autenticado cuando resetea password
    public async Task<IActionResult> ChangePassword([FromBody] PasswordChangeDto dto)
    {
        var response = new ResponseApi<bool>();
        try
        {
            // Validaciones
            if (string.IsNullOrEmpty(dto.ResetToken))
            {
                response.IsSuccess = false;
                response.Message = "Token requerido";
                response.ErrorCode = "TOKEN_REQUIRED";
                return BadRequest(response);
            }

            if (string.IsNullOrEmpty(dto.NewPassword) || dto.NewPassword.Length < 8)
            {
                response.IsSuccess = false;
                response.Message = "La contraseña debe tener al menos 8 caracteres";
                response.ErrorCode = "WEAK_PASSWORD";
                return BadRequest(response);
            }

            response.Data = await _passwordService.ChangePasswordAsync(dto);
            response.IsSuccess = true;
            response.Message = "Contraseña actualizada correctamente. Ya puedes iniciar sesión.";

            return Ok(response);
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.ErrorCode = "PASSWORD_CHANGE_ERROR";
            response.Message = ex.Message == "Token de reset inválido o ya usado"
                ? "El enlace es inválido o ya fue utilizado. Solicita uno nuevo."
                : ex.Message == "El token ha expirado"
                ? "El enlace ha expirado. Solicita uno nuevo (válido por 30 minutos)."
                : "Error al cambiar la contraseña. Intenta nuevamente.";

            return BadRequest(response);
        }
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}