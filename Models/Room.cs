using System.Text.Json.Serialization;

namespace room_for_lease_api.Models
{
    public class Room
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Address { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public double Area { get; set; }
        public bool IsAvailable { get; set; } = true;

        public int OwnerId { get; set; }
        [JsonIgnore]
        public User Owner { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}



