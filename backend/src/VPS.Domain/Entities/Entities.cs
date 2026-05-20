using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VPS.Domain.Enums;

namespace VPS.Domain.Entities;

public class User
{
    public int Id { get; set; }
    [Required, MaxLength(150)] public string Email { get; set; } = null!;
    [Required] public string PasswordHash { get; set; } = null!;
    [Required, MaxLength(20)] public string Role { get; set; } = null!;
    [MaxLength(20)] public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Customer? Customer { get; set; }
    public Staff? Staff { get; set; }
}

public class Staff
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    [Required, MaxLength(100)] public string FullName { get; set; } = null!;
    [MaxLength(100)] public string? Position { get; set; }
    public decimal? Salary { get; set; }
    public DateTime HiredAt { get; set; } = DateTime.UtcNow;
}

public class Customer
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    [Required, MaxLength(100)] public string FullName { get; set; } = null!;
    [MaxLength(250)] public string? Address { get; set; }
    [MaxLength(50)] public string? NationalId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public ICollection<SalesInvoice> SalesInvoices { get; set; } = new List<SalesInvoice>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<PartRequest> PartRequests { get; set; } = new List<PartRequest>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}

public class Vehicle
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    [Required, MaxLength(20)] public string PlateNo { get; set; } = null!;
    [MaxLength(50)] public string? Make { get; set; }
    [MaxLength(50)] public string? Model { get; set; }
    public int? Year { get; set; }
    public int MileageKm { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Vendor
{
    public int Id { get; set; }
    [Required, MaxLength(150)] public string Name { get; set; } = null!;
    [MaxLength(50)] public string? Contact { get; set; }
    [MaxLength(150)] public string? Email { get; set; }
    [MaxLength(250)] public string? Address { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Part> Parts { get; set; } = new List<Part>();
    public ICollection<PurchaseInvoice> PurchaseInvoices { get; set; } = new List<PurchaseInvoice>();
}

public class Category
{
    public int Id { get; set; }
    [Required, MaxLength(100)] public string Name { get; set; } = null!;
    public ICollection<Part> Parts { get; set; } = new List<Part>();
}

public class Part
{
    public int Id { get; set; }
    [Required, MaxLength(150)] public string Name { get; set; } = null!;
    [Required, MaxLength(50)] public string SKU { get; set; } = null!;
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }
    public int? VendorId { get; set; }
    public Vendor? Vendor { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal BuyPrice { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal SellPrice { get; set; }
    public int Stock { get; set; }
    public int? AvgLifespanKm { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class PurchaseInvoice
{
    public int Id { get; set; }
    public int VendorId { get; set; }
    public Vendor Vendor { get; set; } = null!;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    [Column(TypeName = "decimal(18,2)")] public decimal Total { get; set; }
    public int CreatedByUserId { get; set; }
    public ICollection<PurchaseInvoiceItem> Items { get; set; } = new List<PurchaseInvoiceItem>();
}

public class PurchaseInvoiceItem
{
    public int Id { get; set; }
    public int PurchaseInvoiceId { get; set; }
    public PurchaseInvoice PurchaseInvoice { get; set; } = null!;
    public int PartId { get; set; }
    public Part Part { get; set; } = null!;
    public int Qty { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal UnitCost { get; set; }
}

public class SalesInvoice
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public int? VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    [Column(TypeName = "decimal(18,2)")] public decimal SubTotal { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Discount { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Total { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public DateTime? CreditDueDate { get; set; }
    public int StaffUserId { get; set; }
    public ICollection<SalesInvoiceItem> Items { get; set; } = new List<SalesInvoiceItem>();
}

public class SalesInvoiceItem
{
    public int Id { get; set; }
    public int SalesInvoiceId { get; set; }
    public SalesInvoice SalesInvoice { get; set; } = null!;
    public int PartId { get; set; }
    public Part Part { get; set; } = null!;
    public int Qty { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal UnitPrice { get; set; }
}

public class Appointment
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;
    public DateTime ScheduledAt { get; set; }
    public AppointmentStatus Status { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }
}

public class PartRequest
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    [Required, MaxLength(150)] public string PartName { get; set; } = null!;
    [MaxLength(500)] public string? Note { get; set; }
    public PartRequestStatus Status { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
}

public class Review
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public int Rating { get; set; }
    [MaxLength(1000)] public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class StockAlert
{
    public int Id { get; set; }
    public int PartId { get; set; }
    public Part Part { get; set; } = null!;
    public DateTime AlertedAt { get; set; } = DateTime.UtcNow;
    public bool Resolved { get; set; }
}

public class AuditLog
{
    public int Id { get; set; }
    [MaxLength(100)] public string Action { get; set; } = null!;
    [MaxLength(100)] public string Entity { get; set; } = null!;
    public int? EntityId { get; set; }
    public int? UserId { get; set; }
    [MaxLength(2000)] public string? Detail { get; set; }
    public DateTime At { get; set; } = DateTime.UtcNow;
}
