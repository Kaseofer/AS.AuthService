using AgendaSalud.AuthService.Application.DTOs;
using AgendaSalud.AuthService.Application.Interfaces;
using AgendaSalud.AuthService.Domain.Entities;

namespace AgendaSalud.AuthService.Application.Services
{
    public class ExternalAuthService : IExternalAuthService
    {
        private readonly IGenericRepository<ExternalLogin> _externalLoginRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<Role> _roleRepository;
        private readonly IJwtGeneratorService _jwtGenerator;

        // Constante para usuarios sin contraseña
        private const string OAUTH_ONLY_MARKER = "OAUTH_ONLY_NO_PASSWORD";

        public ExternalAuthService(
            IGenericRepository<ExternalLogin> externalLoginRepository,
            IGenericRepository<User> userRepository,
            IGenericRepository<Role> roleRepository,
            IJwtGeneratorService jwtGenerator)
        {
            _externalLoginRepository = externalLoginRepository;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _jwtGenerator = jwtGenerator;
        }

        public async Task<AuthResponseDto> LoginOrRegisterAsync(ExternalLoginDto dto)
        {
            // 1. Buscar si ya existe el login externo vinculado
            var existingExternalLogin = await _externalLoginRepository.QueryAsync(
                e => e.Provider == dto.Provider && e.ExternalId == dto.ProviderId,
                includeProperties: "User.Role");

            if (existingExternalLogin.Any())
            {
                var user = existingExternalLogin.First().User;

                if (!user.IsActive)
                {
                    throw new UnauthorizedAccessException("Usuario inactivo. Contacte al administrador.");
                }

                if (user.IsLocked)
                {
                    if (user.NextAllowedLogin.HasValue && DateTime.UtcNow >= user.NextAllowedLogin.Value)
                    {
                        user.IsLocked = false;
                        user.FailedAttempts = 0;
                        user.NextAllowedLogin = null;
                    }
                    else
                    {
                        var remainingTime = user.NextAllowedLogin.HasValue
                            ? (user.NextAllowedLogin.Value - DateTime.UtcNow).Minutes
                            : 15;
                        throw new UnauthorizedAccessException(
                            $"Cuenta bloqueada. Intente nuevamente en {remainingTime} minutos");
                    }
                }

                user.LastSuccessfulLogin = DateTime.UtcNow;
                user.FailedAttempts = 0;
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

            // 2. Verificar si ya existe un usuario con ese email
            var existingUserByEmail = await _userRepository.QueryAsync(
                u => u.Email == dto.Email,
                includeProperties: "Role");

            if (existingUserByEmail.Any())
            {
                // Usuario existe (registrado con password) - vincular OAuth
                var user = existingUserByEmail.First();

                if (!user.IsActive)
                {
                    throw new UnauthorizedAccessException("Usuario inactivo. Contacte al administrador.");
                }

                if (user.IsLocked)
                {
                    if (user.NextAllowedLogin.HasValue && DateTime.UtcNow >= user.NextAllowedLogin.Value)
                    {
                        user.IsLocked = false;
                        user.FailedAttempts = 0;
                        user.NextAllowedLogin = null;
                    }
                    else
                    {
                        var remainingTime = user.NextAllowedLogin.HasValue
                            ? (user.NextAllowedLogin.Value - DateTime.UtcNow).Minutes
                            : 15;
                        throw new UnauthorizedAccessException(
                            $"Cuenta bloqueada. Intente nuevamente en {remainingTime} minutos");
                    }
                }

                // Vincular OAuth a cuenta existente
                var newExternalLogin = new ExternalLogin
                {
                    Id = Guid.NewGuid(),
                    Provider = dto.Provider,
                    ExternalId = dto.ProviderId,
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow
                };

                await _externalLoginRepository.AddAsync(newExternalLogin);

                user.LastSuccessfulLogin = DateTime.UtcNow;
                user.FailedAttempts = 0;
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

            // 3. Usuario nuevo - crear con OAuth (sin contraseña por ahora)
            var defaultRole = await GetDefaultRoleAsync();

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                FullName = dto.FullName,

                // Marcar que no tiene contraseña aún (solo OAuth)
                // Si modificaste la tabla para permitir NULL, usa: PasswordHash = null
                // Si no, usa un marcador especial:
                PasswordHash = OAUTH_ONLY_MARKER,

                RoleId = defaultRole.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                LastSuccessfulLogin = DateTime.UtcNow,
                FailedAttempts = 0,
                IsLocked = false,
                ForcePasswordChange = false,
                LastFailedLogin = null,
                NextAllowedLogin = null,
                PasswordChangedAt = null
            };

            var createdUser = await _userRepository.AddAsync(newUser);

            if (createdUser == null)
                throw new Exception("No se pudo crear el usuario");

            var externalLogin = new ExternalLogin
            {
                Id = Guid.NewGuid(),
                Provider = dto.Provider,
                ExternalId = dto.ProviderId,
                UserId = createdUser.Id,
                CreatedAt = DateTime.UtcNow
            };

            await _externalLoginRepository.AddAsync(externalLogin);

            createdUser.Role = defaultRole;
            var tokenFinal = _jwtGenerator.GenerateToken(createdUser);

            return new AuthResponseDto
            {
                UserId = createdUser.Id,
                Email = createdUser.Email,
                FullName = createdUser.FullName,
                Role = defaultRole.Name,
                Token = tokenFinal,
                ExpiresAt = DateTime.UtcNow.AddHours(2),
                ForcePasswordChange = false
            };
        }

        private async Task<Role> GetDefaultRoleAsync()
        {
            var roles = await _roleRepository.QueryAsync(r => r.Name == "Patient");

            if (!roles.Any())
                throw new Exception("Rol 'Patient' no encontrado en la base de datos");

            return roles.First();
        }
    }
}