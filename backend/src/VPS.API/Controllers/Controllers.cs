using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VPS.Application.DTOs;
using VPS.Application.Interfaces;
using VPS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace VPS.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _svc;
    public AuthController(IAuthService svc) => _svc = svc;

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var r = await _svc.LoginAsync(dto);
        return r == null ? Unauthorized(new { error = "Invalid credentials" }) : Ok(r);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterCustomerDto dto)
        => Ok(await _svc.RegisterCustomerAsync(dto));
}

[ApiController]
[Route("api/staff")]
[Authorize(Policy = "AdminOnly")]
public class StaffController : ControllerBase
{
    private readonly IStaffService _svc;
    public StaffController(IStaffService svc) => _svc = svc;

    [HttpGet] public async Task<IActionResult> List() => Ok(await _svc.ListAsync());
    [HttpPost] public async Task<IActionResult> Create(CreateStaffDto dto) => Ok(await _svc.CreateAsync(dto));
    [HttpPut("{id}")] public async Task<IActionResult> Update(int id, UpdateStaffDto dto)
        => (await _svc.UpdateAsync(id, dto)) is { } r ? Ok(r) : NotFound();
    [HttpDelete("{id}")] public async Task<IActionResult> Delete(int id)
        => await _svc.DeleteAsync(id) ? NoContent() : NotFound();
}

[ApiController]
[Route("api/vendors")]
[Authorize(Policy = "AdminOnly")]
public class VendorsController : ControllerBase
{
    private readonly IVendorService _svc;
    public VendorsController(IVendorService svc) => _svc = svc;

    [HttpGet] public async Task<IActionResult> List() => Ok(await _svc.ListAsync());
    [HttpPost] public async Task<IActionResult> Create(CreateVendorDto dto) => Ok(await _svc.CreateAsync(dto));
    [HttpPut("{id}")] public async Task<IActionResult> Update(int id, CreateVendorDto dto)
        => (await _svc.UpdateAsync(id, dto)) is { } r ? Ok(r) : NotFound();
    [HttpDelete("{id}")] public async Task<IActionResult> Delete(int id)
        => await _svc.DeleteAsync(id) ? NoContent() : NotFound();
}

[ApiController]
[Route("api/parts")]
[Authorize]
public class PartsController : ControllerBase
{
    private readonly IPartService _svc;
    public PartsController(IPartService svc) => _svc = svc;

    [HttpGet] public async Task<IActionResult> List([FromQuery] string? q) => Ok(await _svc.ListAsync(q));
    [HttpGet("{id}")] public async Task<IActionResult> Get(int id)
        => (await _svc.GetAsync(id)) is { } r ? Ok(r) : NotFound();

    [HttpPost, Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create(CreatePartDto dto) => Ok(await _svc.CreateAsync(dto));

    [HttpPut("{id}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Update(int id, CreatePartDto dto)
        => (await _svc.UpdateAsync(id, dto)) is { } r ? Ok(r) : NotFound();

    [HttpDelete("{id}"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(int id) => await _svc.DeleteAsync(id) ? NoContent() : NotFound();
}

[ApiController]
[Route("api/inventory")]
[Authorize(Policy = "AdminOnly")]
public class InventoryController : ControllerBase
{
    private readonly IPartService _svc;
    public InventoryController(IPartService svc) => _svc = svc;

    [HttpGet("low-stock")] public async Task<IActionResult> Low() => Ok(await _svc.LowStockAsync());
}

[ApiController]
[Route("api/categories")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;
    public CategoriesController(AppDbContext db) => _db = db;

    [HttpGet] public async Task<IActionResult> List() =>
        Ok(await _db.Categories.Select(c => new CategoryDto(c.Id, c.Name)).ToListAsync());

    [HttpPost, Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create([FromBody] CategoryDto dto)
    {
        var c = new VPS.Domain.Entities.Category { Name = dto.Name };
        _db.Categories.Add(c);
        await _db.SaveChangesAsync();
        return Ok(new CategoryDto(c.Id, c.Name));
    }
}

[ApiController]
[Route("api/customers")]
[Authorize(Policy = "StaffOrAdmin")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _svc;
    public CustomersController(ICustomerService svc) => _svc = svc;

    [HttpGet("search")] public async Task<IActionResult> Search([FromQuery] string q) => Ok(await _svc.SearchAsync(q));
    [HttpGet("{id}")] public async Task<IActionResult> Get(int id)
        => (await _svc.GetAsync(id)) is { } r ? Ok(r) : NotFound();
    [HttpPost] public async Task<IActionResult> Create(CreateCustomerByStaffDto dto) => Ok(await _svc.CreateByStaffAsync(dto));
    [HttpGet("{id}/history")] public async Task<IActionResult> History(int id) => Ok(await _svc.HistoryAsync(id));
    [HttpPost("{id}/vehicles")] public async Task<IActionResult> AddVehicle(int id, CreateVehicleDto dto)
        => Ok(await _svc.AddVehicleAsync(id, dto));
}

[ApiController]
[Route("api/purchase-invoices")]
[Authorize(Policy = "AdminOnly")]
public class PurchasesController : ControllerBase
{
    private readonly IPurchaseService _svc;
    public PurchasesController(IPurchaseService svc) => _svc = svc;

    [HttpGet] public async Task<IActionResult> List() => Ok(await _svc.ListAsync());
    [HttpPost]
    public async Task<IActionResult> Create(CreatePurchaseInvoiceDto dto)
    {
        var uid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _svc.CreateAsync(dto, uid));
    }
}

[ApiController]
[Route("api/sales-invoices")]
[Authorize(Policy = "StaffOrAdmin")]
public class SalesController : ControllerBase
{
    private readonly ISalesService _svc;
    public SalesController(ISalesService svc) => _svc = svc;

    [HttpGet] public async Task<IActionResult> List() => Ok(await _svc.ListAsync());

    [HttpPost]
    public async Task<IActionResult> Create(CreateSalesInvoiceDto dto)
    {
        var uid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _svc.CreateAsync(dto, uid));
    }

    [HttpPost("{id}/email")]
    public async Task<IActionResult> Email(int id) =>
        await _svc.EmailInvoiceAsync(id) ? Ok(new { sent = true }) : NotFound();
}

[ApiController]
[Route("api/reports")]
[Authorize(Policy = "StaffOrAdmin")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _svc;
    public ReportsController(IReportService svc) => _svc = svc;

    [HttpGet("financial"), Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Financial([FromQuery] string period = "monthly")
        => Ok(await _svc.FinancialAsync(period));

    [HttpGet("customers")]
    public async Task<IActionResult> Customers([FromQuery] string type = "top")
    {
        return type.ToLower() switch
        {
            "top" => Ok(await _svc.TopSpendersAsync()),
            "regular" => Ok(await _svc.RegularsAsync()),
            "credit-pending" => Ok(await _svc.PendingCreditsAsync()),
            _ => BadRequest(new { error = "Invalid type" })
        };
    }
}

[ApiController]
[Route("api/appointments")]
[Authorize]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _svc;
    private readonly ICustomerService _cust;
    public AppointmentsController(IAppointmentService svc, ICustomerService cust)
    { _svc = svc; _cust = cust; }

    [HttpGet, Authorize(Policy = "StaffOrAdmin")]
    public async Task<IActionResult> All() => Ok(await _svc.AllAsync());

    [HttpPost, Authorize(Policy = "CustomerOnly")]
    public async Task<IActionResult> Create(CreateAppointmentDto dto)
    {
        var uid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var c = await _cust.GetByUserAsync(uid) ?? throw new InvalidOperationException("Customer profile missing");
        return Ok(await _svc.CreateAsync(c.Id, dto));
    }
}

[ApiController]
[Route("api/part-requests")]
[Authorize]
public class PartRequestsController : ControllerBase
{
    private readonly IPartRequestService _svc;
    private readonly ICustomerService _cust;
    public PartRequestsController(IPartRequestService svc, ICustomerService cust)
    { _svc = svc; _cust = cust; }

    [HttpGet, Authorize(Policy = "StaffOrAdmin")]
    public async Task<IActionResult> All() => Ok(await _svc.AllAsync());

    [HttpPost, Authorize(Policy = "CustomerOnly")]
    public async Task<IActionResult> Create(CreatePartRequestDto dto)
    {
        var uid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var c = await _cust.GetByUserAsync(uid) ?? throw new InvalidOperationException("Customer profile missing");
        return Ok(await _svc.CreateAsync(c.Id, dto));
    }
}

[ApiController]
[Route("api/reviews")]
[Authorize]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _svc;
    private readonly ICustomerService _cust;
    public ReviewsController(IReviewService svc, ICustomerService cust) { _svc = svc; _cust = cust; }

    [HttpGet, AllowAnonymous] public async Task<IActionResult> All() => Ok(await _svc.AllAsync());

    [HttpPost, Authorize(Policy = "CustomerOnly")]
    public async Task<IActionResult> Create(CreateReviewDto dto)
    {
        var uid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var c = await _cust.GetByUserAsync(uid) ?? throw new InvalidOperationException("Customer profile missing");
        return Ok(await _svc.CreateAsync(c.Id, dto));
    }
}

[ApiController]
[Route("api/profile")]
[Authorize(Policy = "CustomerOnly")]
public class ProfileController : ControllerBase
{
    private readonly ICustomerService _cust;
    private readonly IAppointmentService _appt;
    private readonly IPartRequestService _req;
    private readonly IFailurePredictionService _ml;
    private readonly AppDbContext _db;

    public ProfileController(ICustomerService cust, IAppointmentService appt,
        IPartRequestService req, IFailurePredictionService ml, AppDbContext db)
    { _cust = cust; _appt = appt; _req = req; _ml = ml; _db = db; }

    int Uid => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet] public async Task<IActionResult> Me()
        => (await _cust.GetByUserAsync(Uid)) is { } r ? Ok(r) : NotFound();

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] CustomerUpdateDto dto)
    {
        var c = await _cust.GetByUserAsync(Uid) ?? throw new InvalidOperationException("Profile missing");
        await _cust.UpdateProfileAsync(c.Id, dto.FullName, dto.Address, dto.Phone);
        return Ok();
    }

    [HttpGet("vehicles")] public async Task<IActionResult> Vehicles()
    {
        var c = await _cust.GetByUserAsync(Uid);
        return c == null ? NotFound() : Ok(c.Vehicles);
    }

    [HttpPost("vehicles")] public async Task<IActionResult> AddVehicle(CreateVehicleDto dto)
    {
        var c = await _cust.GetByUserAsync(Uid) ?? throw new InvalidOperationException("Profile missing");
        return Ok(await _cust.AddVehicleAsync(c.Id, dto));
    }

    [HttpGet("history")] public async Task<IActionResult> History()
    {
        var c = await _cust.GetByUserAsync(Uid);
        return c == null ? NotFound() : Ok(await _cust.HistoryAsync(c.Id));
    }

    [HttpGet("appointments")] public async Task<IActionResult> Appts()
    {
        var c = await _cust.GetByUserAsync(Uid);
        return c == null ? NotFound() : Ok(await _appt.ByCustomerAsync(c.Id));
    }

    [HttpGet("part-requests")] public async Task<IActionResult> Reqs()
    {
        var c = await _cust.GetByUserAsync(Uid);
        return c == null ? NotFound() : Ok(await _req.ByCustomerAsync(c.Id));
    }

    [HttpGet("predict-failure/{vehicleId}")]
    public async Task<IActionResult> Predict(int vehicleId, [FromQuery] string partType = "Brake",
        [FromQuery] float monthsInstalled = 12, [FromQuery] float avgLifespanKm = 50000)
    {
        var v = await _db.Vehicles.FirstOrDefaultAsync(x => x.Id == vehicleId);
        if (v == null) return NotFound();
        var dto = new FailurePredictDto(vehicleId, partType, v.MileageKm, monthsInstalled, avgLifespanKm);
        return Ok(_ml.Predict(dto));
    }
}

public record CustomerUpdateDto(string FullName, string? Address, string? Phone);
