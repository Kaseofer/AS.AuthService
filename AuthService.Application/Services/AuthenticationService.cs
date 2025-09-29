using AgendaSalud.AuthService.Application.DTOs;
using AgendaSalud.AuthService.Application.Interfaces;
using AgendaSalud.AuthService.Domain.Entities;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AgendaSalud.AuthService.Application.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IGenericRepository<Role> _RoleRepository;
        private readonly IGenericRepository<User> _UserRepository;

        private readonly IJwtGeneratorService _jwtGenerator;

        public AuthenticationService(IGenericRepository<Role> roleRepository,IGenericRepository<User> userRepository, IJwtGeneratorService jwtGenerator)
        {
            
            _RoleRepository = roleRepository;
            _UserRepository = userRepository;

            _jwtGenerator = jwtGenerator;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterUserDto dto)
        {
        
            try
            {
                //Buscando si el Role Existe
                var listroles = await _RoleRepository.QueryAsync(r => r.Name.Equals(dto.RoleName));

                if (!listroles.Any())
                {
                    throw new TaskCanceledException("Role not found");
                }

                var role = listroles.First();


                //Checkeando si Ya esta registrado.
                var Existe = await _UserRepository.GetAsync(u => u.Email == dto.Email);

                if (Existe != null)
                {
                    throw new TaskCanceledException("Ya esta Registrado " + dto.Email);
                }

                var user = new User
                {
                    Email = dto.Email,
                    PasswordHash = HashPassword(dto.Password),
                    FullName = dto.FullName,
                    RoleId = role.Id
                };
                
                var nuevoUsuario = await _UserRepository.AddAsync(user);

                if (nuevoUsuario == null)
                {
                    throw new TaskCanceledException("Error Creando el Usuario");
                }

                // Generar token JWT
                var token = _jwtGenerator.GenerateToken(user);

                return new AuthResponseDto
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
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
                var users = await _UserRepository.QueryAsync(u => u.Email == dto.Email, includeProperties: "Role");

                if (!users.Any())
                {
                    throw new TaskCanceledException("No encontro el usuario");
                }

                var user = users.First();

                if (user == null || !VerifyPassword(dto.Password, user.PasswordHash))
                    throw new TaskCanceledException("Credenciales Invalidas");

                var token = _jwtGenerator.GenerateToken(user);

                return new AuthResponseDto
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = user.Role.Name,
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddHours(2)
                };
            }
            catch 
            {

                throw;
            }
            
        }

        private string HashPassword(string password)
        {
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

        public async Task<AuthResponseDto> GetCurrentUserAsync(string token)
        {
            try
            {
                var principal = _jwtGenerator.ValidateToken(token);

                if (principal == null)
                {
                    throw new TaskCanceledException("Token inválido");
                }

                // Extraer el UserId del token como Guid
                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
                {
                    throw new TaskCanceledException("Token no contiene información válida del usuario");
                }

                // Buscar el usuario con Guid
                var users = await _UserRepository.QueryAsync(u => u.Id == userId, includeProperties: "Role");

                if (!users.Any())
                {
                    throw new TaskCanceledException("Usuario no encontrado");
                }

                var user = users.First();

                return new AuthResponseDto
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = user.Role.Name,
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddHours(2)
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
                // Validar el token usando el servicio JWT
                var principal = _jwtGenerator.ValidateToken(token);

                if (principal == null)
                {
                    return new TokenValidationDto
                    {
                        IsValid = false,
                        Email = string.Empty,
                        Role = string.Empty,
                        ExpiresAt = DateTime.MinValue
                    };
                }

                // Extraer información del token
                var userIdClaim = principal.FindFirst("UserId")?.Value;
                var emailClaim = principal.FindFirst("Email")?.Value;
                var roleClaim = principal.FindFirst("Role")?.Value;
                var expClaim = principal.FindFirst("exp")?.Value;

                // Convertir la fecha de expiración
                DateTime expiresAt = DateTime.MinValue;
                if (!string.IsNullOrEmpty(expClaim) && long.TryParse(expClaim, out long exp))
                {
                    expiresAt = DateTimeOffset.FromUnixTimeSeconds(exp).DateTime;
                }

                // Verificar si el token ha expirado
                bool isValid = expiresAt > DateTime.UtcNow;

                return new TokenValidationDto
                {
                    IsValid = isValid,
                    UserId = Guid.TryParse(userIdClaim, out Guid userId) ? userId : Guid.Empty,
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
                    Email = string.Empty,
                    Role = string.Empty,
                    ExpiresAt = DateTime.MinValue
                };
            }
        }
    }
}