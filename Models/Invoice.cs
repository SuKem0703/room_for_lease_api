using System.Text.Json.Serialization;

namespace room_for_lease_api.Models
{
    public enum InvoiceStatus
    {
        Pending = 1,
        Paid = 2,
        Overdue = 3
    }

    public class Invoice
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        [JsonIgnore]
        public Room Room { get; set; } = null!;

        public int ContractId { get; set; }
        [JsonIgnore]
        public Contract Contract { get; set; } = null!;

        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Period { get; set; } = string.Empty; // Format: "MM/YYYY" or "11/2024"

        // Điện (Electricity)
        public decimal? ElectricOldReading { get; set; }
        public decimal? ElectricNewReading { get; set; }
        public decimal? ElectricConsumption { get; set; } // kWh
        public decimal ElectricUnitPrice { get; set; }
        public decimal ElectricCost { get; set; }

        // Nước (Water)
        public decimal? WaterOldReading { get; set; }
        public decimal? WaterNewReading { get; set; }
        public decimal? WaterConsumption { get; set; } // m³
        public decimal WaterUnitPrice { get; set; }
        public decimal WaterCost { get; set; }

        // Rác + Wifi (Trash + Wifi)
        public decimal ServiceCost { get; set; } // Rác + wifi
        public decimal ServiceUnitPrice { get; set; }

        // Tiền phòng (Room Rent)
        public decimal RoomRent { get; set; }
        public decimal RoomRentUnitPrice { get; set; }

        // Cọc phòng (Deposit - thông tin tham khảo, không tính vào tổng tháng này)
        public decimal? Deposit { get; set; }

        // Tổng tiền thanh toán
        public decimal Amount { get; set; } // Total = ElectricCost + WaterCost + ServiceCost + RoomRent

        // Thanh toán
        public decimal? PaidAmount { get; set; }
        public DateTime? PaidDate { get; set; }
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
