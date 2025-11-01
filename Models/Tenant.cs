using System.Text.Json.Serialization;

namespace room_for_lease_api.Models
{
    public class Tenant
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        [JsonIgnore]
        public User? User { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Address { get; set; }

        [JsonIgnore]
        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
}


