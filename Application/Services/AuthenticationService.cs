using AgendaSalud.AuthService.Application.DTOs;
using AgendaSalud.AuthService.Application.Interfaces;
using AgendaSalud.AuthService.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AgendaSalud.AuthService.Application.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IGenericRepository<Role> _roleRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IJwtGeneratorService _jwtGenerator;

        // Configuración de seguridad
        private const int MAX_FAILED_ATTEMPTS = 5;
        private const int LOCKOUT_MINUTES = 15;

        public AuthenticationService(
            IGenericRepository<Role> roleRepository,
            IGenericRepository<User> userRepository,
            IJwtGeneratorService jwtGenerator)
        {
            _roleRepository = roleRepository;
            _userRepository = userRepository;
            _jwtGenerator = jwtGenerator;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterUserDto dto)
        {
            try
            {
                // Buscar si el Role existe
                var roles = await _roleRepository.QueryAsync(r => r.Name.Equals(dto.RoleName));

                if (!roles.Any())
                {
                    throw new Exception($"Rol '{dto.RoleName}' no encontrado");
                }

                var role = roles.First();

                // Verificar si ya está registrado
                var existingUser = await _userRepository.GetAsync(u => u.Email == dto.Email);

                if (existingUser != null)
                {
                    throw new Exception($"El email {dto.Email} ya está registrado");
                }

                // Crear nuevo usuario
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = dto.Email,
                    PasswordHash = HashPassword(dto.Password),
                    FullName = dto.FullName,
                    RoleId = role.Id,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    PasswordChangedAt = DateTime.UtcNow,
                    FailedAttempts = 0,
                    IsLocked = false,
                    ForcePasswordChange = false
                };

                var newUser = await _userRepository.AddAsync(user);

                if (newUser == null)
                {
                    throw new Exception("Error creando el usuario");
                }

                // Asignar el role al usuario para el token
                newUser.Role = role;

                // Generar token JWT
                var token = _jwtGenerator.GenerateToken(newUser);

                return new AuthResponseDto
                {
                    UserId = newUser.Id,
                    Email = newUser.Email,
                    FullName = newUser.FullName,
                    Role = role.Name,
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddHours(2)
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<AuthResponseDto> LoginAsync(LoginUserDto dto)
        {
            try
            {
                var users = await _userRepository.QueryAsync(
                    u => u.Email == dto.Email,
                    includeProperties: "Role");

                if (!users.Any())
                {
                    throw new UnauthorizedAccessException("Credenciales inválidas");
                }

                var user = users.First();

                // Verificar si el usuario está activo
                if (!user.IsActive)
                {
                    throw new UnauthorizedAccessException("Usuario inactivo");
                }

                // Verificar si la cuenta está bloqueada
                if (user.IsLocked)
                {
                    // Verificar si el período de bloqueo ya expiró
                    if (user.NextAllowedLogin.HasValue && DateTime.UtcNow >= user.NextAllowedLogin.Value)
                    {
                        // Desbloquear cuenta
                        user.IsLocked = false;
                        user.FailedAttempts = 0;
                        user.NextAllowedLogin = null;
                        await _userRepository.UpdateAsync(user);
                    }
                    else
                    {
                        var remainingTime = user.NextAllowedLogin.HasValue
                            ? (user.NextAllowedLogin.Value - DateTime.UtcNow).Minutes
                            : LOCKOUT_MINUTES;
                        throw new UnauthorizedAccessException(
                            $"Cuenta bloqueada. Intente nuevamente en {remainingTime} minutos");
                    }
                }

                // Verificar contraseña
                if (!VerifyPassword(dto.Password, user.PasswordHash))
                {
                    // Incrementar intentos fallidos
                    user.FailedAttempts++;
                    user.LastFailedLogin = DateTime.UtcNow;

                    // Bloquear si excede el máximo de intentos
                    if (user.FailedAttempts >= MAX_FAILED_ATTEMPTS)
                    {
                        user.IsLocked = true;
                        user.NextAllowedLogin = DateTime.UtcNow.AddMinutes(LOCKOUT_MINUTES);
                    }

                    await _userRepository.UpdateAsync(user);

                    throw new UnauthorizedAccessException(
                        $"Credenciales inválidas. Intentos restantes: {MAX_FAILED_ATTEMPTS - user.FailedAttempts}");
                }

                // Login exitoso - resetear intentos fallidos
                user.FailedAttempts = 0;
                user.LastSuccessfulLogin = DateTime.UtcNow;
                user.LastFailedLogin = null;
                user.IsLocked = false;
                user.NextAllowedLogin = null;
                await _userRepository.UpdateAsync(user);

                var token = _jwtGenerator.GenerateToken(user);

                return new AuthResponseDto
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = user.Role.Name,
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddHours(2),
                    ForcePasswordChange = user.ForcePasswordChange
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<AuthResponseDto> GetCurrentUserAsync(string token)
        {
            try
            {
                var principal = _jwtGenerator.ValidateToken(token);

                if (principal == null)
                {
                    throw new UnauthorizedAccessException("Token inválido");
                }

                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                {
                    throw new UnauthorizedAccessException("Token no contiene información válida del usuario");
                }

                var users = await _userRepository.QueryAsync(
                    u => u.Id == userId,
                    includeProperties: "Role");

                if (!users.Any())
                {
                    throw new UnauthorizedAccessException("Usuario no encontrado");
                }

                var user = users.First();

                if (!user.IsActive || user.IsLocked)
                {
                    throw new UnauthorizedAccessException("Usuario inactivo o bloqueado");
                }

                return new AuthResponseDto
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = user.Role.Name,
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddHours(2),
                    ForcePasswordChange = user.ForcePasswordChange
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<TokenValidationDto> ValidateTokenAsync(string token)
        {
            try
            {
                var principal = _jwtGenerator.ValidateToken(token);

                if (principal == null)
                {
                    return new TokenValidationDto
                    {
                        IsValid = false,
                        UserId = Guid.Empty,
                        Email = string.Empty,
                        Role = string.Empty,
                        ExpiresAt = DateTime.MinValue
                    };
                }

                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var emailClaim = principal.FindFirst(ClaimTypes.Email)?.Value
                                ?? principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value;
                var roleClaim = principal.FindFirst(ClaimTypes.Role)?.Value;
                var expClaim = principal.FindFirst(JwtRegisteredClaimNames.Exp)?.Value
                              ?? principal.FindFirst("exp")?.Value;

                DateTime expiresAt = DateTime.MinValue;
                if (!string.IsNullOrEmpty(expClaim) && long.TryParse(expClaim, out long exp))
                {
                    expiresAt = DateTimeOffset.FromUnixTimeSeconds(exp).DateTime;
                }

                bool isValid = expiresAt > DateTime.UtcNow;

                // Verificar estado del usuario
                if (isValid && Guid.TryParse(userIdClaim, out Guid userId))
                {
                    var user = await _userRepository.GetAsync(u => u.Id == userId);
                    if (user == null || !user.IsActive || user.IsLocked)
                    {
                        isValid = false;
                    }
                }

                return new TokenValidationDto
                {
                    IsValid = isValid,
                    UserId = Guid.TryParse(userIdClaim, out Guid parsedUserId) ? parsedUserId : Guid.Empty,
                    Email = emailClaim ?? string.Empty,
                    Role = roleClaim ?? string.Empty,
                    ExpiresAt = expiresAt
                };
            }
            catch
            {
                return new TokenValidationDto
                {
                    IsValid = false,
                    UserId = Guid.Empty,
                    Email = string.Empty,
                    Role = string.Empty,
                    ExpiresAt = DateTime.MinValue
                };
            }
        }

        private string HashPassword(string password)
        {
            // TODO: Cambiar a BCrypt en producción
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == storedHash;
        }
    }
}