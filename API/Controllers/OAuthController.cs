using AgendaSalud.AuthService.Application.DTOs;
using AgendaSalud.AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;

[ApiController]
[Route("oauth")]
[AllowAnonymous]
public class OAuthController : ControllerBase
{
    private readonly IExternalAuthService _externalAuthService;
    private readonly IConfiguration _configuration;

    public OAuthController(IExternalAuthService externalAuthService, IConfiguration configuration)
    {
        _externalAuthService = externalAuthService;
        _configuration = configuration;
    }

    // Iniciar flujo OAuth con Google
    [HttpGet("google")]
    public IActionResult GoogleLogin(string returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(GoogleCallback), "OAuth");
        var properties = new AuthenticationProperties
        {
            RedirectUri = redirectUrl,
            Items = { { "returnUrl", returnUrl ?? _configuration["Frontend:Url"] } }
        };

        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    // Callback de Google
    [HttpGet("google-callback")]
    public async Task<IActionResult> GoogleCallback()
    {
        var frontendUrl = _configuration["Frontend:Url"] ?? "http://localhost:4200";

        try
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                return Redirect($"{frontendUrl}/auth/error?error=google_auth_failed");
            }

            var email = result.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = result.Principal.FindFirst(ClaimTypes.Name)?.Value;
            var googleId = result.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                return Redirect($"{frontendUrl}/auth/error?error=no_email");
            }

            // Usar tu servicio existente
            var authResponse = await _externalAuthService.LoginOrRegisterAsync(new ExternalLoginDto
            {
                Provider = "Google",
                Email = email,
                FullName = name,
                ProviderId = googleId
            });

            return Redirect($"{frontendUrl}/auth/success?token={authResponse.Token}");
        }
        catch (Exception ex)
        {
            return Redirect($"{frontendUrl}/auth/error?error={Uri.EscapeDataString(ex.Message)}");
        }
    }
}