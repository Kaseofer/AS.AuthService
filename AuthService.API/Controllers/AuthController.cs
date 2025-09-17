using AgendaSalud.AuthService.Api.Common;
using AgendaSalud.AuthService.Application.DTOs;
using AgendaSalud.AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;

    public AuthController(IAuthenticationService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
    {
        var response = new ResponseApi<AuthResponseDto>();

        try
        {
            response.IsSuccess = true;
            response.Message = "Registro de Usuario Exitoso";
            response.Data = await _authService.RegisterAsync(dto); 

            return Ok(response);
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.Message = ex.Message;
            response.ErrorCode = "REGISTRATION_ERROR";

            return StatusCode(500, response);
        }
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserDto dto)
    {
        var response = new ResponseApi<AuthResponseDto>();
        try
        {
            response.IsSuccess = true;
            response.Message = "Login Exitoso";
            response.Data = await _authService.LoginAsync(dto); ;

            return Ok(response);
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.Message = ex.Message;
            response.ErrorCode = "LOGIN_ERROR";

            // Logueo opcional en audit_log
            return Unauthorized(response);
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        var user = await _authService.GetCurrentUserAsync(token);

        if (user == null)
        {
            return Unauthorized(new ResponseApi<object>
            {
                IsSuccess = false,
                Message = "Token inválido o usuario no encontrado",
                ErrorCode = "INVALID_TOKEN"
            });
        }

        return Ok(new ResponseApi<AuthResponseDto>
        {
            IsSuccess = true,
            Message = "Usuario autenticado",
            Data = user
        });
    }

    [HttpGet("validate-token")]
    public async Task<IActionResult> ValidateToken()
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        var result = await _authService.ValidateTokenAsync(token);

        if (result == null || !result.IsValid)
        {
            return Unauthorized(new ResponseApi<object>
            {
                IsSuccess = false,
                Message = "Token inválido",
                ErrorCode = "INVALID_TOKEN"
            });
        }

        return Ok(new ResponseApi<TokenValidationDto>
        {
            IsSuccess = true,
            Message = "Token válido",
            Data = result
        });
    }


}
