using Microsoft.EntityFrameworkCore;

namespace room_for_lease_api.Models
{
    public static class SeedData
    {
        public static async Task SeedAsync(AppDbContext db, IConfiguration config)
        {
            await db.Database.MigrateAsync();

            if (!db.Users.Any())
            {
                var seedSection = config.GetSection("Seed");
                var ownerEmail = seedSection["OwnerEmail"] ?? "owner@demo.com";
                var tenantEmail = seedSection["TenantEmail"] ?? "tenant@demo.com";
                var password = seedSection["Password"] ?? "123456";

                var admin = new User
                {
                    Email = "admin@demo.com",
                    FullName = "Administrator",
                    Role = UserRole.Admin,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456")
                };

                var owner = new User
                {
                    Email = ownerEmail,
                    FullName = "Owner Demo",
                    Role = UserRole.Owner,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
                };

                var tenantUser = new User
                {
                    Email = tenantEmail,
                    FullName = "Tenant Demo",
                    Role = UserRole.Tenant,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
                };

                db.Users.AddRange(admin, owner, tenantUser);
                await db.SaveChangesAsync();

                if (!db.Rooms.Any())
                {
                    db.Rooms.Add(new Room
                    {
                        Title = "Phòng trọ trung tâm",
                        Address = "123 Nguyễn Trãi, Q1, TP.HCM",
                        Description = "Gần chợ, an ninh tốt",
                        Price = 3500000,
                        Area = 18,
                        IsAvailable = true,
                        OwnerId = owner.Id
                    });
                    await db.SaveChangesAsync();
                }

                if (!db.Tenants.Any())
                {
                    var sampleTenant = new Tenant
                    {
                        FullName = "Nguyễn Văn A",
                        Phone = "+84 912345678",
                        Email = "tenant.person@demo.com",
                        Address = "456 Lê Lợi, Q1, TP.HCM"
                    };
                    db.Tenants.Add(sampleTenant);
                    await db.SaveChangesAsync();

                    var firstRoom = db.Rooms.First();
                    db.Contracts.Add(new Contract
                    {
                        TenantId = sampleTenant.Id,
                        RoomId = firstRoom.Id,
                        StartDate = DateTime.UtcNow.Date.AddDays(-7),
                        MonthlyRent = firstRoom.Price,
                        Status = ContractStatus.Active
                    });
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}


