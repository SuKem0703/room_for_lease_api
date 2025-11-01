using System.Text.Json.Serialization;

namespace room_for_lease_api.Models
{
    public enum ContractStatus
    {
        Active = 1,
        Terminated = 2
    }

    public class Contract
    {
        public int Id { get; set; }
        public int TenantId { get; set; }
        [JsonIgnore]
        public Tenant Tenant { get; set; } = null!;

        public int RoomId { get; set; }
        public Room Room { get; set; } = null!;

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal MonthlyRent { get; set; }
        public ContractStatus Status { get; set; } = ContractStatus.Active;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? TerminatedAt { get; set; }
    }
}


