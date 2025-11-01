using System.Text.Json.Serialization;

namespace room_for_lease_api.Models
{
    public enum UserRole
    {
        Admin = 0,
        Owner = 1,
        Tenant = 2
    }

    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        [JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;

        [JsonIgnore]
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
    }
}


