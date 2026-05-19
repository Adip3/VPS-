using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using VPS.Domain.Entities;
using VPS.Domain.Enums;

namespace VPS.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        if (!await db.Users.AnyAsync(u => u.Role == Roles.Admin))
        {
            var admin = new User
            {
                Email = "admin@vps.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = Roles.Admin,
                Phone = "9800000001"
            };
            db.Users.Add(admin);
            await db.SaveChangesAsync();
        }

        if (!await db.Users.AnyAsync(u => u.Email == "staff@vps.com"))
        {
            var staffUser = new User
            {
                Email = "staff@vps.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff@123"),
                Role = Roles.Staff,
                Phone = "9800000002"
            };
            db.Users.Add(staffUser);
            await db.SaveChangesAsync();
            db.Staff.Add(new Staff { UserId = staffUser.Id, FullName = "Default Staff", Position = "Sales", Salary = 30000 });
            await db.SaveChangesAsync();
        }

        if (!await db.Categories.AnyAsync())
        {
            db.Categories.AddRange(
                new Category { Name = "Engine" },
                new Category { Name = "Brakes" },
                new Category { Name = "Filters" },
                new Category { Name = "Electrical" },
                new Category { Name = "Tyres" });
            await db.SaveChangesAsync();
        }

        if (!await db.Vendors.AnyAsync())
        {
            db.Vendors.AddRange(
                new Vendor { Name = "AutoMax Suppliers", Contact = "9811111111", Email = "automax@v.com", Address = "Kathmandu" },
                new Vendor { Name = "PartHub Nepal", Contact = "9822222222", Email = "parthub@v.com", Address = "Lalitpur" });
            await db.SaveChangesAsync();
        }

        if (!await db.Parts.AnyAsync())
        {
            var v1 = await db.Vendors.FirstAsync();
            var c1 = await db.Categories.FirstAsync();
            db.Parts.AddRange(
                new Part { Name = "Brake Pad Set", SKU = "BP-001", CategoryId = c1.Id, VendorId = v1.Id, BuyPrice = 1500, SellPrice = 2200, Stock = 25, AvgLifespanKm = 50000 },
                new Part { Name = "Oil Filter", SKU = "OF-001", CategoryId = c1.Id, VendorId = v1.Id, BuyPrice = 400, SellPrice = 700, Stock = 50, AvgLifespanKm = 10000 },
                new Part { Name = "Spark Plug", SKU = "SP-001", CategoryId = c1.Id, VendorId = v1.Id, BuyPrice = 250, SellPrice = 450, Stock = 8, AvgLifespanKm = 30000 });
            await db.SaveChangesAsync();
        }
    }
}
