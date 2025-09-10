using AgendaSalud.AuthService.Application.DTOs;
using AgendaSalud.AuthService.Application.Interfaces;
using AgendaSalud.AuthService.Domain.Entities;

namespace AgendaSalud.AuthService.Application.Services
{
    public class ExternalAuthService : IExternalAuthService
    {
        private readonly IGenericRepository<ExternalLogin> _ExternalLoginRepository;
        private readonly IGenericRepository<User> _UserRepository;
        private readonly IGenericRepository<Role> _RoleRepository;

        private readonly IJwtGeneratorService _jwtGenerator;

        public ExternalAuthService(IGenericRepository<ExternalLogin> _externalLoginRepository
                                  ,IGenericRepository<User> _userRepository
                                  ,IGenericRepository<Role> _roleRepository
                                  , IJwtGeneratorService jwtGenerator)
        { 
            _ExternalLoginRepository = _externalLoginRepository;
            _UserRepository = _userRepository;
            _RoleRepository = _roleRepository;
        }

        public async Task<AuthResponseDto> LoginOrRegisterAsync(ExternalLoginDto dto)
        {
            // Buscar si ya existe el login externo
            var existing = await _ExternalLoginRepository.QueryAsync(e =>
                e.Provider == dto.Provider && e.ExternalId == dto.ExternalId, includeProperties: "User.Role");

            if (existing.Any())
            {
                var user = existing.First().User;

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

            // Si no existe, registrar nuevo usuario
            var newUser = new User
            {
                Email = dto.Email,
                FullName = dto.FullName,
                PasswordHash = null, // No se usa en login externo
                RoleId = await GetDefaultRoleIdAsync() // Ej: "Patient" o "Professional"
            };

            var usuarioIntCreado = await _UserRepository.AddAsync(newUser);
            

            if (usuarioIntCreado == null)
                throw new TaskCanceledException("No se pudo crear el usuario interno");

            var externalLogin = new ExternalLogin
            {
                Provider = dto.Provider,
                ExternalId = dto.ExternalId,
                UserId = usuarioIntCreado.Id
            };

            var usuarioExtNuevo = await _ExternalLoginRepository.AddAsync(externalLogin);
            if (usuarioExtNuevo == null)
                throw new TaskCanceledException("No se pudo crear el usuario externo");

            var tokenFinal = _jwtGenerator.GenerateToken(usuarioIntCreado);

            return new AuthResponseDto
            {
                UserId = usuarioIntCreado.Id,
                Email = usuarioIntCreado.Email,
                FullName = usuarioIntCreado.FullName,
                Role =  (await GetDefaultRoleIdAsync()).ToString(), // O el que corresponda
                Token = tokenFinal,
                ExpiresAt = DateTime.UtcNow.AddHours(2)
            };
        }

        private async Task<Guid> GetDefaultRoleIdAsync()
        {
            var roles = await _RoleRepository.GetAllAsync();

            var rolPaciente = roles.FirstOrDefault(r => r.Name == "Patient");

            return rolPaciente!.Id;

        }
    }
}
