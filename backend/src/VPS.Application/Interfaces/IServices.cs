using VPS.Application.DTOs;
using VPS.Domain.Entities;

namespace VPS.Application.Interfaces;

public interface ITokenService { string CreateToken(User u); }

public interface IEmailSender
{
    Task SendAsync(string to, string subject, string body, byte[]? attachment = null, string? attachName = null);
    Task SendAdminAsync(string subject, string body);
}

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginDto dto);
    Task<AuthResponse> RegisterCustomerAsync(RegisterCustomerDto dto);
}

public interface IStaffService
{
    Task<List<StaffDto>> ListAsync();
    Task<StaffDto> CreateAsync(CreateStaffDto dto);
    Task<StaffDto?> UpdateAsync(int id, UpdateStaffDto dto);
    Task<bool> DeleteAsync(int id);
}

public interface IVendorService
{
    Task<List<VendorDto>> ListAsync();
    Task<VendorDto> CreateAsync(CreateVendorDto dto);
    Task<VendorDto?> UpdateAsync(int id, CreateVendorDto dto);
    Task<bool> DeleteAsync(int id);
}

public interface IPartService
{
    Task<List<PartDto>> ListAsync(string? q);
    Task<PartDto?> GetAsync(int id);
    Task<PartDto> CreateAsync(CreatePartDto dto);
    Task<PartDto?> UpdateAsync(int id, CreatePartDto dto);
    Task<bool> DeleteAsync(int id);
    Task<List<LowStockDto>> LowStockAsync();
}

public interface ICustomerService
{
    Task<CustomerDto?> GetAsync(int id);
    Task<CustomerDto?> GetByUserAsync(int userId);
    Task<List<CustomerDto>> SearchAsync(string q);
    Task<CustomerDto> CreateByStaffAsync(CreateCustomerByStaffDto dto);
    Task<List<SalesInvoiceDto>> HistoryAsync(int customerId);
    Task<List<VehicleDto>> AddVehicleAsync(int customerId, CreateVehicleDto dto);
    Task<bool> UpdateProfileAsync(int customerId, string fullName, string? address, string? phone);
}

public interface IPurchaseService
{
    Task<PurchaseInvoiceDto> CreateAsync(CreatePurchaseInvoiceDto dto, int userId);
    Task<List<PurchaseInvoiceDto>> ListAsync();
}

public interface ISalesService
{
    Task<SalesInvoiceDto> CreateAsync(CreateSalesInvoiceDto dto, int staffUserId);
    Task<bool> EmailInvoiceAsync(int invoiceId);
    Task<List<SalesInvoiceDto>> ListAsync();
    decimal CalcDiscount(decimal subtotal);
}

public interface IReportService
{
    Task<FinancialReportDto> FinancialAsync(string period);
    Task<List<CustomerReportDto>> TopSpendersAsync(int n = 10);
    Task<List<CustomerReportDto>> RegularsAsync(int minVisits = 3);
    Task<List<CustomerReportDto>> PendingCreditsAsync();
}

public interface IAppointmentService
{
    Task<AppointmentDto> CreateAsync(int customerId, CreateAppointmentDto dto);
    Task<List<AppointmentDto>> ByCustomerAsync(int customerId);
    Task<List<AppointmentDto>> AllAsync();
}

public interface IPartRequestService
{
    Task<PartRequestDto> CreateAsync(int customerId, CreatePartRequestDto dto);
    Task<List<PartRequestDto>> AllAsync();
    Task<List<PartRequestDto>> ByCustomerAsync(int customerId);
}

public interface IReviewService
{
    Task<ReviewDto> CreateAsync(int customerId, CreateReviewDto dto);
    Task<List<ReviewDto>> AllAsync();
}

public interface IFailurePredictionService
{
    FailurePredictResultDto Predict(FailurePredictDto dto);
}
