using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VPS.Application.DTOs;
using VPS.Application.Interfaces;
using VPS.Domain.Entities;
using VPS.Domain.Enums;
using VPS.Infrastructure.Data;

namespace VPS.Infrastructure.Repositories;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly ITokenService _tok;
    public AuthService(AppDbContext db, ITokenService tok) { _db = db; _tok = tok; }

    public async Task<AuthResponse?> LoginAsync(LoginDto dto)
    {
        var u = await _db.Users.FirstOrDefaultAsync(x => x.Email == dto.Email && x.IsActive);
        if (u == null) return null;
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, u.PasswordHash)) return null;
        return new AuthResponse(_tok.CreateToken(u), u.Role, u.Email, u.Id);
    }

    public async Task<AuthResponse> RegisterCustomerAsync(RegisterCustomerDto dto)
    {
        if (await _db.Users.AnyAsync(x => x.Email == dto.Email))
            throw new InvalidOperationException("Email already registered");
        var u = new User
        {
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = Roles.Customer,
            Phone = dto.Phone
        };
        _db.Users.Add(u);
        await _db.SaveChangesAsync();
        _db.Customers.Add(new Customer
        {
            UserId = u.Id, FullName = dto.FullName, Address = dto.Address, NationalId = dto.NationalId
        });
        await _db.SaveChangesAsync();
        return new AuthResponse(_tok.CreateToken(u), u.Role, u.Email, u.Id);
    }
}

public class StaffService : IStaffService
{
    private readonly AppDbContext _db;
    public StaffService(AppDbContext db) => _db = db;

    public async Task<List<StaffDto>> ListAsync() =>
        await _db.Staff.Include(s => s.User)
            .Select(s => new StaffDto(s.Id, s.User.Email, s.FullName, s.User.Phone, s.Position, s.Salary, s.HiredAt))
            .ToListAsync();

    public async Task<StaffDto> CreateAsync(CreateStaffDto dto)
    {
        if (await _db.Users.AnyAsync(x => x.Email == dto.Email))
            throw new InvalidOperationException("Email exists");
        var u = new User
        {
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = Roles.Staff,
            Phone = dto.Phone
        };
        _db.Users.Add(u);
        await _db.SaveChangesAsync();
        var s = new Staff { UserId = u.Id, FullName = dto.FullName, Position = dto.Position, Salary = dto.Salary };
        _db.Staff.Add(s);
        await _db.SaveChangesAsync();
        return new StaffDto(s.Id, u.Email, s.FullName, u.Phone, s.Position, s.Salary, s.HiredAt);
    }

    public async Task<StaffDto?> UpdateAsync(int id, UpdateStaffDto dto)
    {
        var s = await _db.Staff.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
        if (s == null) return null;
        s.FullName = dto.FullName;
        s.Position = dto.Position;
        s.Salary = dto.Salary;
        s.User.Phone = dto.Phone;
        s.User.IsActive = dto.IsActive;
        await _db.SaveChangesAsync();
        return new StaffDto(s.Id, s.User.Email, s.FullName, s.User.Phone, s.Position, s.Salary, s.HiredAt);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var s = await _db.Staff.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == id);
        if (s == null) return false;
        _db.Users.Remove(s.User);
        await _db.SaveChangesAsync();
        return true;
    }
}

public class VendorService : IVendorService
{
    private readonly AppDbContext _db;
    public VendorService(AppDbContext db) => _db = db;

    public async Task<List<VendorDto>> ListAsync() =>
        await _db.Vendors.Select(v => new VendorDto(v.Id, v.Name, v.Contact, v.Email, v.Address)).ToListAsync();

    public async Task<VendorDto> CreateAsync(CreateVendorDto dto)
    {
        var v = new Vendor { Name = dto.Name, Contact = dto.Contact, Email = dto.Email, Address = dto.Address };
        _db.Vendors.Add(v);
        await _db.SaveChangesAsync();
        return new VendorDto(v.Id, v.Name, v.Contact, v.Email, v.Address);
    }

    public async Task<VendorDto?> UpdateAsync(int id, CreateVendorDto dto)
    {
        var v = await _db.Vendors.FindAsync(id);
        if (v == null) return null;
        v.Name = dto.Name; v.Contact = dto.Contact; v.Email = dto.Email; v.Address = dto.Address;
        await _db.SaveChangesAsync();
        return new VendorDto(v.Id, v.Name, v.Contact, v.Email, v.Address);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var v = await _db.Vendors.FindAsync(id);
        if (v == null) return false;
        _db.Vendors.Remove(v);
        await _db.SaveChangesAsync();
        return true;
    }
}

public class PartService : IPartService
{
    private readonly AppDbContext _db;
    public PartService(AppDbContext db) => _db = db;

    public async Task<List<PartDto>> ListAsync(string? q)
    {
        var query = _db.Parts.Include(p => p.Category).Include(p => p.Vendor).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(p => p.Name.Contains(q) || p.SKU.Contains(q));
        return await query.Select(p => new PartDto(p.Id, p.Name, p.SKU, p.CategoryId, p.Category!.Name,
            p.VendorId, p.Vendor!.Name, p.BuyPrice, p.SellPrice, p.Stock, p.AvgLifespanKm)).ToListAsync();
    }

    public async Task<PartDto?> GetAsync(int id)
    {
        var p = await _db.Parts.Include(x => x.Category).Include(x => x.Vendor).FirstOrDefaultAsync(x => x.Id == id);
        return p == null ? null : new PartDto(p.Id, p.Name, p.SKU, p.CategoryId, p.Category?.Name, p.VendorId, p.Vendor?.Name, p.BuyPrice, p.SellPrice, p.Stock, p.AvgLifespanKm);
    }

    public async Task<PartDto> CreateAsync(CreatePartDto dto)
    {
        var p = new Part
        {
            Name = dto.Name, SKU = dto.SKU, CategoryId = dto.CategoryId, VendorId = dto.VendorId,
            BuyPrice = dto.BuyPrice, SellPrice = dto.SellPrice, Stock = dto.Stock, AvgLifespanKm = dto.AvgLifespanKm
        };
        _db.Parts.Add(p);
        await _db.SaveChangesAsync();
        return (await GetAsync(p.Id))!;
    }

    public async Task<PartDto?> UpdateAsync(int id, CreatePartDto dto)
    {
        var p = await _db.Parts.FindAsync(id);
        if (p == null) return null;
        p.Name = dto.Name; p.SKU = dto.SKU; p.CategoryId = dto.CategoryId; p.VendorId = dto.VendorId;
        p.BuyPrice = dto.BuyPrice; p.SellPrice = dto.SellPrice; p.Stock = dto.Stock; p.AvgLifespanKm = dto.AvgLifespanKm;
        await _db.SaveChangesAsync();
        return await GetAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var p = await _db.Parts.FindAsync(id);
        if (p == null) return false;
        _db.Parts.Remove(p);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<LowStockDto>> LowStockAsync() =>
        await _db.Parts.Where(p => p.Stock < 10)
            .Select(p => new LowStockDto(p.Id, p.Name, p.SKU, p.Stock)).ToListAsync();
}

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _db;
    public CustomerService(AppDbContext db) => _db = db;

    private CustomerDto Map(Customer c) =>
        new(c.Id, c.UserId, c.User.Email, c.FullName, c.User.Phone, c.Address, c.NationalId,
            c.Vehicles.Select(v => new VehicleDto(v.Id, v.CustomerId, v.PlateNo, v.Make, v.Model, v.Year, v.MileageKm)).ToList());

    public async Task<CustomerDto?> GetAsync(int id)
    {
        var c = await _db.Customers.Include(x => x.User).Include(x => x.Vehicles).FirstOrDefaultAsync(x => x.Id == id);
        return c == null ? null : Map(c);
    }

    public async Task<CustomerDto?> GetByUserAsync(int userId)
    {
        var c = await _db.Customers.Include(x => x.User).Include(x => x.Vehicles).FirstOrDefaultAsync(x => x.UserId == userId);
        return c == null ? null : Map(c);
    }

    public async Task<List<CustomerDto>> SearchAsync(string q)
    {
        q = (q ?? "").Trim();
        var query = _db.Customers.Include(c => c.User).Include(c => c.Vehicles).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(c =>
                c.FullName.Contains(q) ||
                (c.User.Phone != null && c.User.Phone.Contains(q)) ||
                (c.NationalId != null && c.NationalId.Contains(q)) ||
                c.Vehicles.Any(v => v.PlateNo.Contains(q)));
        var list = await query.Take(50).ToListAsync();
        return list.Select(Map).ToList();
    }

    public async Task<CustomerDto> CreateByStaffAsync(CreateCustomerByStaffDto dto)
    {
        if (await _db.Users.AnyAsync(x => x.Email == dto.Email))
            throw new InvalidOperationException("Email exists");
        var u = new User
        {
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = Roles.Customer,
            Phone = dto.Phone
        };
        _db.Users.Add(u);
        await _db.SaveChangesAsync();
        var c = new Customer { UserId = u.Id, FullName = dto.FullName, Address = dto.Address, NationalId = dto.NationalId };
        _db.Customers.Add(c);
        await _db.SaveChangesAsync();
        foreach (var v in dto.Vehicles)
            _db.Vehicles.Add(new Vehicle { CustomerId = c.Id, PlateNo = v.PlateNo, Make = v.Make, Model = v.Model, Year = v.Year, MileageKm = v.MileageKm });
        await _db.SaveChangesAsync();
        return (await GetAsync(c.Id))!;
    }

    public async Task<List<SalesInvoiceDto>> HistoryAsync(int customerId)
    {
        var list = await _db.SalesInvoices
            .Where(s => s.CustomerId == customerId)
            .Include(s => s.Items).ThenInclude(i => i.Part)
            .Include(s => s.Customer)
            .OrderByDescending(s => s.Date)
            .ToListAsync();

        return list.Select(s => new SalesInvoiceDto(s.Id, s.CustomerId, s.Customer.FullName, s.Date,
            s.SubTotal, s.Discount, s.Total, s.PaymentStatus.ToString(),
            s.Items.Select(i => new SalesInvoiceItemDto(i.PartId, i.Part.Name, i.Qty, i.UnitPrice, i.Qty * i.UnitPrice)).ToList())).ToList();
    }

    public async Task<List<VehicleDto>> AddVehicleAsync(int customerId, CreateVehicleDto dto)
    {
        var c = await _db.Customers.Include(x => x.Vehicles).FirstOrDefaultAsync(x => x.Id == customerId);
        if (c == null) throw new InvalidOperationException("Customer not found");
        c.Vehicles.Add(new Vehicle { PlateNo = dto.PlateNo, Make = dto.Make, Model = dto.Model, Year = dto.Year, MileageKm = dto.MileageKm });
        await _db.SaveChangesAsync();
        return c.Vehicles.Select(v => new VehicleDto(v.Id, v.CustomerId, v.PlateNo, v.Make, v.Model, v.Year, v.MileageKm)).ToList();
    }

    public async Task<bool> UpdateProfileAsync(int customerId, string fullName, string? address, string? phone)
    {
        var c = await _db.Customers.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == customerId);
        if (c == null) return false;
        c.FullName = fullName; c.Address = address;
        if (phone != null) c.User.Phone = phone;
        await _db.SaveChangesAsync();
        return true;
    }
}

public class PurchaseService : IPurchaseService
{
    private readonly AppDbContext _db;
    public PurchaseService(AppDbContext db) => _db = db;

    public async Task<PurchaseInvoiceDto> CreateAsync(CreatePurchaseInvoiceDto dto, int userId)
    {
        var inv = new PurchaseInvoice { VendorId = dto.VendorId, CreatedByUserId = userId };
        decimal total = 0;
        foreach (var it in dto.Items)
        {
            var p = await _db.Parts.FindAsync(it.PartId);
            if (p == null) throw new InvalidOperationException($"Part {it.PartId} missing");
            inv.Items.Add(new PurchaseInvoiceItem { PartId = it.PartId, Qty = it.Qty, UnitCost = it.UnitCost });
            p.Stock += it.Qty;
            total += it.Qty * it.UnitCost;
        }
        inv.Total = total;
        _db.PurchaseInvoices.Add(inv);
        await _db.SaveChangesAsync();
        var v = await _db.Vendors.FindAsync(dto.VendorId);
        return new PurchaseInvoiceDto(inv.Id, inv.VendorId, v!.Name, inv.Date, inv.Total,
            inv.Items.Select(i => new PurchaseItemDto(i.PartId, i.Qty, i.UnitCost)).ToList());
    }

    public async Task<List<PurchaseInvoiceDto>> ListAsync()
    {
        var list = await _db.PurchaseInvoices.Include(p => p.Vendor).Include(p => p.Items).OrderByDescending(p => p.Date).ToListAsync();
        return list.Select(p => new PurchaseInvoiceDto(p.Id, p.VendorId, p.Vendor.Name, p.Date, p.Total,
            p.Items.Select(i => new PurchaseItemDto(i.PartId, i.Qty, i.UnitCost)).ToList())).ToList();
    }
}

public class SalesService : ISalesService
{
    private readonly AppDbContext _db;
    private readonly IEmailSender _email;
    private readonly ILogger<SalesService> _log;

    public SalesService(AppDbContext db, IEmailSender email, ILogger<SalesService> log)
    { _db = db; _email = email; _log = log; }

    public decimal CalcDiscount(decimal subtotal) => subtotal > 5000m ? Math.Round(subtotal * 0.10m, 2) : 0m;

    public async Task<SalesInvoiceDto> CreateAsync(CreateSalesInvoiceDto dto, int staffUserId)
    {
        using var tx = await _db.Database.BeginTransactionAsync();
        var inv = new SalesInvoice
        {
            CustomerId = dto.CustomerId,
            VehicleId = dto.VehicleId,
            StaffUserId = staffUserId,
            PaymentStatus = Enum.Parse<PaymentStatus>(dto.PaymentStatus, true)
        };
        decimal sub = 0;
        foreach (var it in dto.Items)
        {
            var p = await _db.Parts.FindAsync(it.PartId)
                ?? throw new InvalidOperationException($"Part {it.PartId} missing");
            if (p.Stock < it.Qty) throw new InvalidOperationException($"Insufficient stock: {p.Name}");
            inv.Items.Add(new SalesInvoiceItem { PartId = p.Id, Qty = it.Qty, UnitPrice = p.SellPrice });
            p.Stock -= it.Qty;
            sub += p.SellPrice * it.Qty;
        }
        inv.SubTotal = sub;
        inv.Discount = CalcDiscount(sub);
        inv.Total = sub - inv.Discount;
        if (inv.PaymentStatus == PaymentStatus.Credit)
            inv.CreditDueDate = DateTime.UtcNow.AddMonths(1);
        _db.SalesInvoices.Add(inv);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        _log.LogInformation("Sale invoice {Id} total {Total}", inv.Id, inv.Total);

        var c = await _db.Customers.FindAsync(dto.CustomerId);
        return new SalesInvoiceDto(inv.Id, inv.CustomerId, c!.FullName, inv.Date,
            inv.SubTotal, inv.Discount, inv.Total, inv.PaymentStatus.ToString(),
            inv.Items.Select(i => new SalesInvoiceItemDto(i.PartId,
                _db.Parts.Find(i.PartId)!.Name, i.Qty, i.UnitPrice, i.Qty * i.UnitPrice)).ToList());
    }

    public async Task<bool> EmailInvoiceAsync(int invoiceId)
    {
        var inv = await _db.SalesInvoices
            .Include(s => s.Customer).ThenInclude(c => c.User)
            .Include(s => s.Items).ThenInclude(i => i.Part)
            .FirstOrDefaultAsync(s => s.Id == invoiceId);
        if (inv == null) return false;

        var html = $@"<h2>Invoice #{inv.Id}</h2>
<p>Customer: {inv.Customer.FullName}<br/>Date: {inv.Date:yyyy-MM-dd HH:mm}</p>
<table border='1' cellpadding='6' cellspacing='0'>
<tr><th>Part</th><th>Qty</th><th>Price</th><th>Total</th></tr>
{string.Join("", inv.Items.Select(i => $"<tr><td>{i.Part.Name}</td><td>{i.Qty}</td><td>{i.UnitPrice}</td><td>{i.Qty * i.UnitPrice}</td></tr>"))}
</table>
<p><b>Subtotal:</b> NPR {inv.SubTotal}<br/>
<b>Discount:</b> NPR {inv.Discount}<br/>
<b>Total:</b> NPR {inv.Total}<br/>
<b>Status:</b> {inv.PaymentStatus}</p>";
        await _email.SendAsync(inv.Customer.User.Email, $"Invoice #{inv.Id} — Vehicle Parts", html);
        return true;
    }

    public async Task<List<SalesInvoiceDto>> ListAsync()
    {
        var list = await _db.SalesInvoices
            .Include(s => s.Customer).Include(s => s.Items).ThenInclude(i => i.Part)
            .OrderByDescending(s => s.Date).Take(200).ToListAsync();
        return list.Select(s => new SalesInvoiceDto(s.Id, s.CustomerId, s.Customer.FullName, s.Date,
            s.SubTotal, s.Discount, s.Total, s.PaymentStatus.ToString(),
            s.Items.Select(i => new SalesInvoiceItemDto(i.PartId, i.Part.Name, i.Qty, i.UnitPrice, i.Qty * i.UnitPrice)).ToList())).ToList();
    }
}

public class ReportService : IReportService
{
    private readonly AppDbContext _db;
    public ReportService(AppDbContext db) => _db = db;

    public async Task<FinancialReportDto> FinancialAsync(string period)
    {
        DateTime from = period.ToLower() switch
        {
            "daily" => DateTime.UtcNow.Date,
            "monthly" => new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1),
            "yearly" => new DateTime(DateTime.UtcNow.Year, 1, 1),
            _ => DateTime.UtcNow.Date
        };
        var to = DateTime.UtcNow;
        var sales = await _db.SalesInvoices.Where(s => s.Date >= from && s.Date <= to).ToListAsync();
        var purchases = await _db.PurchaseInvoices.Where(p => p.Date >= from && p.Date <= to).ToListAsync();
        var totalSales = sales.Sum(s => s.Total);
        var totalPurchases = purchases.Sum(p => p.Total);
        return new FinancialReportDto(period, from, to, totalSales, totalPurchases, totalSales - totalPurchases, sales.Count);
    }

    public async Task<List<CustomerReportDto>> TopSpendersAsync(int n = 10)
    {
        var data = await _db.Customers.Include(c => c.User).Include(c => c.SalesInvoices).ToListAsync();
        return data.Select(c => new CustomerReportDto(c.Id, c.FullName, c.User.Phone,
            c.SalesInvoices.Sum(s => s.Total), c.SalesInvoices.Count,
            c.SalesInvoices.Where(s => s.PaymentStatus == PaymentStatus.Credit).Sum(s => s.Total)))
            .OrderByDescending(x => x.TotalSpent).Take(n).ToList();
    }

    public async Task<List<CustomerReportDto>> RegularsAsync(int minVisits = 3)
    {
        var data = await _db.Customers.Include(c => c.User).Include(c => c.SalesInvoices).ToListAsync();
        return data.Where(c => c.SalesInvoices.Count >= minVisits)
            .Select(c => new CustomerReportDto(c.Id, c.FullName, c.User.Phone,
                c.SalesInvoices.Sum(s => s.Total), c.SalesInvoices.Count,
                c.SalesInvoices.Where(s => s.PaymentStatus == PaymentStatus.Credit).Sum(s => s.Total)))
            .OrderByDescending(x => x.VisitCount).ToList();
    }

    public async Task<List<CustomerReportDto>> PendingCreditsAsync()
    {
        var data = await _db.Customers.Include(c => c.User).Include(c => c.SalesInvoices).ToListAsync();
        return data.Select(c => new CustomerReportDto(c.Id, c.FullName, c.User.Phone,
            c.SalesInvoices.Sum(s => s.Total), c.SalesInvoices.Count,
            c.SalesInvoices.Where(s => s.PaymentStatus == PaymentStatus.Credit).Sum(s => s.Total)))
            .Where(x => x.CreditOutstanding > 0).OrderByDescending(x => x.CreditOutstanding).ToList();
    }
}

public class AppointmentService : IAppointmentService
{
    private readonly AppDbContext _db;
    public AppointmentService(AppDbContext db) => _db = db;

    public async Task<AppointmentDto> CreateAsync(int customerId, CreateAppointmentDto dto)
    {
        var v = await _db.Vehicles.FirstOrDefaultAsync(x => x.Id == dto.VehicleId && x.CustomerId == customerId)
            ?? throw new InvalidOperationException("Vehicle not found");
        var a = new Appointment
        {
            CustomerId = customerId, VehicleId = v.Id,
            ScheduledAt = dto.ScheduledAt, Notes = dto.Notes, Status = AppointmentStatus.Pending
        };
        _db.Appointments.Add(a);
        await _db.SaveChangesAsync();
        return new AppointmentDto(a.Id, a.CustomerId, a.VehicleId, v.PlateNo, a.ScheduledAt, a.Status.ToString(), a.Notes);
    }

    public async Task<List<AppointmentDto>> ByCustomerAsync(int customerId) =>
        await _db.Appointments.Include(a => a.Vehicle).Where(a => a.CustomerId == customerId)
            .Select(a => new AppointmentDto(a.Id, a.CustomerId, a.VehicleId, a.Vehicle.PlateNo, a.ScheduledAt, a.Status.ToString(), a.Notes))
            .ToListAsync();

    public async Task<List<AppointmentDto>> AllAsync() =>
        await _db.Appointments.Include(a => a.Vehicle)
            .Select(a => new AppointmentDto(a.Id, a.CustomerId, a.VehicleId, a.Vehicle.PlateNo, a.ScheduledAt, a.Status.ToString(), a.Notes))
            .ToListAsync();
}

public class PartRequestService : IPartRequestService
{
    private readonly AppDbContext _db;
    public PartRequestService(AppDbContext db) => _db = db;

    public async Task<PartRequestDto> CreateAsync(int customerId, CreatePartRequestDto dto)
    {
        var r = new PartRequest { CustomerId = customerId, PartName = dto.PartName, Note = dto.Note, Status = PartRequestStatus.Pending };
        _db.PartRequests.Add(r);
        await _db.SaveChangesAsync();
        return new PartRequestDto(r.Id, r.CustomerId, r.PartName, r.Note, r.Status.ToString(), r.RequestedAt);
    }

    public async Task<List<PartRequestDto>> AllAsync() =>
        await _db.PartRequests.OrderByDescending(x => x.RequestedAt)
            .Select(r => new PartRequestDto(r.Id, r.CustomerId, r.PartName, r.Note, r.Status.ToString(), r.RequestedAt)).ToListAsync();

    public async Task<List<PartRequestDto>> ByCustomerAsync(int customerId) =>
        await _db.PartRequests.Where(x => x.CustomerId == customerId)
            .Select(r => new PartRequestDto(r.Id, r.CustomerId, r.PartName, r.Note, r.Status.ToString(), r.RequestedAt)).ToListAsync();
}

public class ReviewService : IReviewService
{
    private readonly AppDbContext _db;
    public ReviewService(AppDbContext db) => _db = db;

    public async Task<ReviewDto> CreateAsync(int customerId, CreateReviewDto dto)
    {
        var c = await _db.Customers.FindAsync(customerId) ?? throw new InvalidOperationException("Customer missing");
        var r = new Review { CustomerId = customerId, Rating = dto.Rating, Comment = dto.Comment };
        _db.Reviews.Add(r);
        await _db.SaveChangesAsync();
        return new ReviewDto(r.Id, r.CustomerId, c.FullName, r.Rating, r.Comment, r.CreatedAt);
    }

    public async Task<List<ReviewDto>> AllAsync() =>
        await _db.Reviews.Include(r => r.Customer).OrderByDescending(r => r.CreatedAt)
            .Select(r => new ReviewDto(r.Id, r.CustomerId, r.Customer.FullName, r.Rating, r.Comment, r.CreatedAt)).ToListAsync();
}
