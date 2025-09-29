using AgendaSalud.AuthService.Api.Common;
using AgendaSalud.AuthService.Application.DTOs;
using AgendaSalud.AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("external-auth")]
[AllowAnonymous]
public class ExternalAuthController : ControllerBase
{
    private readonly IExternalAuthService _externalAuthService;

    public ExternalAuthController(IExternalAuthService externalAuthService)
    {
        _externalAuthService = externalAuthService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLogin([FromBody] ExternalLoginDto dto)
    {
        var response = new ResponseApi<AuthResponseDto>();
        try
        {
            response.Data = await _externalAuthService.LoginOrRegisterAsync(dto);
            response.IsSuccess = true;
            response.Message = "Login Externo Exitoso";

            return Ok(response);
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.Message = ex.Message;
            response.ErrorCode = "ERROR LOGIN EXTERNO";

            return Unauthorized(response);
        }
        
    }
}
