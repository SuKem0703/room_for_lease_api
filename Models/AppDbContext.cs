using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace room_for_lease_api.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Room> Rooms => Set<Room>();
        public DbSet<Tenant> Tenants => Set<Tenant>();
        public DbSet<Contract> Contracts => Set<Contract>();
        public DbSet<Invoice> Invoices => Set<Invoice>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Email).IsRequired().HasMaxLength(200);
                entity.Property(u => u.FullName).HasMaxLength(200);
            });

            modelBuilder.Entity<Room>(entity =>
            {
                entity.Property(r => r.Title).IsRequired().HasMaxLength(200);
                entity.Property(r => r.Address).IsRequired().HasMaxLength(300);
                entity.Property(r => r.Price).HasPrecision(18, 2);
                entity.HasOne(r => r.Owner)
                      .WithMany(u => u.Rooms)
                      .HasForeignKey(r => r.OwnerId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(r => new { r.IsAvailable, r.Price });
                entity.HasIndex(r => r.CreatedAt);
            });

            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.Property(t => t.FullName).IsRequired().HasMaxLength(200);
                entity.Property(t => t.Phone).IsRequired().HasMaxLength(50);
                entity.Property(t => t.Email).HasMaxLength(200);
                entity.HasOne(t => t.User)
                      .WithMany()
                      .HasForeignKey(t => t.UserId)
                      .OnDelete(DeleteBehavior.SetNull);
                entity.HasIndex(t => t.UserId).IsUnique();
            });

            modelBuilder.Entity<Contract>(entity =>
            {
                entity.Property(c => c.MonthlyRent).HasPrecision(18, 2);
                entity.HasOne(c => c.Tenant)
                      .WithMany(t => t.Contracts)
                      .HasForeignKey(c => c.TenantId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(c => c.Room)
                      .WithMany()
                      .HasForeignKey(c => c.RoomId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(c => new { c.Status, c.StartDate });
            });

            modelBuilder.Entity<Invoice>(entity =>
            {
                // Amount fields
                entity.Property(i => i.Amount).HasPrecision(18, 2);
                entity.Property(i => i.PaidAmount).HasPrecision(18, 2);
                
                // Electricity fields
                entity.Property(i => i.ElectricOldReading).HasPrecision(18, 2);
                entity.Property(i => i.ElectricNewReading).HasPrecision(18, 2);
                entity.Property(i => i.ElectricConsumption).HasPrecision(18, 2);
                entity.Property(i => i.ElectricUnitPrice).HasPrecision(18, 2);
                entity.Property(i => i.ElectricCost).HasPrecision(18, 2);
                
                // Water fields
                entity.Property(i => i.WaterOldReading).HasPrecision(18, 2);
                entity.Property(i => i.WaterNewReading).HasPrecision(18, 2);
                entity.Property(i => i.WaterConsumption).HasPrecision(18, 2);
                entity.Property(i => i.WaterUnitPrice).HasPrecision(18, 2);
                entity.Property(i => i.WaterCost).HasPrecision(18, 2);
                
                // Service fields
                entity.Property(i => i.ServiceCost).HasPrecision(18, 2);
                entity.Property(i => i.ServiceUnitPrice).HasPrecision(18, 2);
                
                // Room rent fields
                entity.Property(i => i.RoomRent).HasPrecision(18, 2);
                entity.Property(i => i.RoomRentUnitPrice).HasPrecision(18, 2);
                
                // Deposit
                entity.Property(i => i.Deposit).HasPrecision(18, 2);
                
                // Other fields
                entity.Property(i => i.Period).IsRequired().HasMaxLength(50);
                entity.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(50);
                
                entity.HasOne(i => i.Room)
                      .WithMany()
                      .HasForeignKey(i => i.RoomId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(i => i.Contract)
                      .WithMany()
                      .HasForeignKey(i => i.ContractId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(i => new { i.RoomId, i.Status });
                entity.HasIndex(i => i.InvoiceNumber).IsUnique();
            });
        }
    }
}


