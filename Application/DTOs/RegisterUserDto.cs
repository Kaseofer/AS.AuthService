namespace AgendaSalud.AuthService.Application.DTOs
{
    public class RegisterUserDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string RoleName { get; set; } = null!; // 'Patient', 'Professional', 'ScheduleManager'
    }

}
