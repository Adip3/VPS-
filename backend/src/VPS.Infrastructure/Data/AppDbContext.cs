using Microsoft.EntityFrameworkCore;
using VPS.Domain.Entities;

namespace VPS.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Vendor> Vendors => Set<Vendor>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<PurchaseInvoice> PurchaseInvoices => Set<PurchaseInvoice>();
    public DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems => Set<PurchaseInvoiceItem>();
    public DbSet<SalesInvoice> SalesInvoices => Set<SalesInvoice>();
    public DbSet<SalesInvoiceItem> SalesInvoiceItems => Set<SalesInvoiceItem>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<PartRequest> PartRequests => Set<PartRequest>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<StockAlert> StockAlerts => Set<StockAlert>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>().HasIndex(x => x.Email).IsUnique();
        b.Entity<Customer>().HasIndex(x => x.NationalId);
        b.Entity<Vehicle>().HasIndex(x => x.PlateNo);
        b.Entity<Part>().HasIndex(x => x.SKU).IsUnique();

        b.Entity<User>().HasOne(u => u.Customer).WithOne(c => c.User)
            .HasForeignKey<Customer>(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<User>().HasOne(u => u.Staff).WithOne(s => s.User)
            .HasForeignKey<Staff>(s => s.UserId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<Customer>().HasMany(c => c.Vehicles).WithOne(v => v.Customer)
            .HasForeignKey(v => v.CustomerId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<SalesInvoice>().HasMany(s => s.Items).WithOne(i => i.SalesInvoice)
            .HasForeignKey(i => i.SalesInvoiceId).OnDelete(DeleteBehavior.Cascade);

        b.Entity<PurchaseInvoice>().HasMany(s => s.Items).WithOne(i => i.PurchaseInvoice)
            .HasForeignKey(i => i.PurchaseInvoiceId).OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(b);
    }
}
