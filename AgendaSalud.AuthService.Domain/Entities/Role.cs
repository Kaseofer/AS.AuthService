using System.Text.Json.Serialization;

namespace AgendaSalud.AuthService.Domain.Entities
{
    public class Role
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!; // 'Patient', 'Professional', 'ScheduleManager'
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}