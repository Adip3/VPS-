namespace VPS.Domain.Enums;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Staff = "Staff";
    public const string Customer = "Customer";
}

public enum PaymentStatus { Paid = 0, Credit = 1, Partial = 2 }
public enum AppointmentStatus { Pending = 0, Confirmed = 1, Completed = 2, Cancelled = 3 }
public enum PartRequestStatus { Pending = 0, Sourced = 1, Rejected = 2 }
