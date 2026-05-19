namespace VPS.Application.DTOs;

public record LoginDto(string Email, string Password);
public record RegisterCustomerDto(string Email, string Password, string FullName, string? Phone, string? Address, string? NationalId);
public record AuthResponse(string Token, string Role, string Email, int UserId);

public record StaffDto(int Id, string Email, string FullName, string? Phone, string? Position, decimal? Salary, DateTime HiredAt);
public record CreateStaffDto(string Email, string Password, string FullName, string? Phone, string? Position, decimal? Salary);
public record UpdateStaffDto(string FullName, string? Phone, string? Position, decimal? Salary, bool IsActive);

public record VendorDto(int Id, string Name, string? Contact, string? Email, string? Address);
public record CreateVendorDto(string Name, string? Contact, string? Email, string? Address);

public record PartDto(int Id, string Name, string SKU, int? CategoryId, string? CategoryName, int? VendorId, string? VendorName, decimal BuyPrice, decimal SellPrice, int Stock, int? AvgLifespanKm);
public record CreatePartDto(string Name, string SKU, int? CategoryId, int? VendorId, decimal BuyPrice, decimal SellPrice, int Stock, int? AvgLifespanKm);

public record CategoryDto(int Id, string Name);

public record VehicleDto(int Id, int CustomerId, string PlateNo, string? Make, string? Model, int? Year, int MileageKm);
public record CreateVehicleDto(string PlateNo, string? Make, string? Model, int? Year, int MileageKm);

public record CustomerDto(int Id, int UserId, string Email, string FullName, string? Phone, string? Address, string? NationalId, List<VehicleDto> Vehicles);
public record CreateCustomerByStaffDto(string Email, string Password, string FullName, string Phone, string? Address, string? NationalId, List<CreateVehicleDto> Vehicles);

public record PurchaseItemDto(int PartId, int Qty, decimal UnitCost);
public record CreatePurchaseInvoiceDto(int VendorId, List<PurchaseItemDto> Items);
public record PurchaseInvoiceDto(int Id, int VendorId, string VendorName, DateTime Date, decimal Total, List<PurchaseItemDto> Items);

public record SaleItemDto(int PartId, int Qty);
public record CreateSalesInvoiceDto(int CustomerId, int? VehicleId, string PaymentStatus, List<SaleItemDto> Items);
public record SalesInvoiceDto(int Id, int CustomerId, string CustomerName, DateTime Date, decimal SubTotal, decimal Discount, decimal Total, string PaymentStatus, List<SalesInvoiceItemDto> Items);
public record SalesInvoiceItemDto(int PartId, string PartName, int Qty, decimal UnitPrice, decimal LineTotal);

public record AppointmentDto(int Id, int CustomerId, int VehicleId, string PlateNo, DateTime ScheduledAt, string Status, string? Notes);
public record CreateAppointmentDto(int VehicleId, DateTime ScheduledAt, string? Notes);

public record PartRequestDto(int Id, int CustomerId, string PartName, string? Note, string Status, DateTime RequestedAt);
public record CreatePartRequestDto(string PartName, string? Note);

public record ReviewDto(int Id, int CustomerId, string CustomerName, int Rating, string? Comment, DateTime CreatedAt);
public record CreateReviewDto(int Rating, string? Comment);

public record FinancialReportDto(string Period, DateTime From, DateTime To, decimal TotalSales, decimal TotalPurchases, decimal Profit, int InvoiceCount);
public record CustomerReportDto(int CustomerId, string FullName, string? Phone, decimal TotalSpent, int VisitCount, decimal CreditOutstanding);
public record LowStockDto(int PartId, string Name, string SKU, int Stock);

public record FailurePredictDto(int VehicleId, string PartType, float MileageKm, float MonthsInstalled, float AvgLifespanKm);
public record FailurePredictResultDto(float FailureProbability, string Recommendation);
