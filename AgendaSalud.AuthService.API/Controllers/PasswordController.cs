using AgendaSalud.AuthService.Api.Common;
using AgendaSalud.AuthService.Application.DTOs;
using AgendaSalud.AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

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
    public async Task<IActionResult> RequestReset([FromBody] PasswordResetRequestDto dto)
    {
        var response = new ResponseApi<bool>();

        try
        {
            response.IsSuccess = true;
            response.Message = "Solicitud de cambio de contraseña registrada";
            response.Data = await _passwordService.RequestResetAsync(dto); ;

            return Ok(response);
        }
        catch( Exception ex)
        {
            response.IsSuccess = false;
            response.ErrorCode = "RESET_REQUEST_ERROR";
            response.Message = ex.Message;

            return StatusCode(500,response);
        }
    }

    [HttpPost("change")]
    public async Task<IActionResult> ChangePassword([FromBody] PasswordChangeDto dto)
    {
        var response = new ResponseApi<bool>();

        try
        {

            response.IsSuccess = true;
            response.Message = "Contraseña actualizada correctamente";
            response.Data = await _passwordService.ChangePasswordAsync(dto); ;

            return Ok(response);
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.ErrorCode = "PASSWORD_CHANGE_ERROR";
            response.Message = ex.Message;

            return StatusCode(500, response);
        }
    }
}
