using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VPS.Application.Interfaces;
using VPS.Domain.Entities;
using VPS.Domain.Enums;
using VPS.Infrastructure.Data;

namespace VPS.Infrastructure.Jobs;

public class StockAlertJob
{
    private readonly AppDbContext _db;
    private readonly IEmailSender _email;
    private readonly ILogger<StockAlertJob> _log;

    public StockAlertJob(AppDbContext db, IEmailSender email, ILogger<StockAlertJob> log)
    { _db = db; _email = email; _log = log; }

    public async Task RunAsync()
    {
        var lowParts = await _db.Parts.Where(p => p.Stock < 10).ToListAsync();
        foreach (var p in lowParts)
        {
            var has = await _db.StockAlerts.AnyAsync(a => a.PartId == p.Id && !a.Resolved);
            if (!has)
            {
                _db.StockAlerts.Add(new StockAlert { PartId = p.Id });
                await _email.SendAdminAsync(
                    $"Low stock: {p.Name}",
                    $"<p>Part <b>{p.Name}</b> (SKU: {p.SKU}) low. Stock: <b>{p.Stock}</b>.</p>");
                _log.LogWarning("Low stock alert: {Name} stock={Stock}", p.Name, p.Stock);
            }
        }
        await _db.SaveChangesAsync();
    }
}

public class CreditReminderJob
{
    private readonly AppDbContext _db;
    private readonly IEmailSender _email;
    private readonly ILogger<CreditReminderJob> _log;

    public CreditReminderJob(AppDbContext db, IEmailSender email, ILogger<CreditReminderJob> log)
    { _db = db; _email = email; _log = log; }

    public async Task RunAsync()
    {
        var cutoff = DateTime.UtcNow.AddMonths(-1);
        var overdue = await _db.SalesInvoices
            .Where(s => s.PaymentStatus == PaymentStatus.Credit && s.Date < cutoff)
            .Include(s => s.Customer).ThenInclude(c => c.User)
            .ToListAsync();

        foreach (var inv in overdue)
        {
            await _email.SendAsync(inv.Customer.User.Email,
                "Payment Overdue Reminder",
                $"<p>Dear {inv.Customer.FullName},</p><p>Invoice <b>#{inv.Id}</b> of total <b>NPR {inv.Total}</b> dated {inv.Date:yyyy-MM-dd} is overdue. Please clear payment.</p>");
            _log.LogInformation("Credit reminder sent invoice={Id}", inv.Id);
        }
    }
}
