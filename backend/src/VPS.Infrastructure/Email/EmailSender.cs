using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using VPS.Application.Interfaces;

namespace VPS.Infrastructure.Email;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _cfg;
    private readonly ILogger<EmailSender> _log;
    public EmailSender(IConfiguration cfg, ILogger<EmailSender> log) { _cfg = cfg; _log = log; }

    public async Task SendAsync(string to, string subject, string body, byte[]? attachment = null, string? attachName = null)
    {
        try
        {
            var msg = new MimeMessage();
            msg.From.Add(MailboxAddress.Parse(_cfg["Smtp:From"]));
            msg.To.Add(MailboxAddress.Parse(to));
            msg.Subject = subject;
            var builder = new BodyBuilder { HtmlBody = body };
            if (attachment != null && attachName != null)
                builder.Attachments.Add(attachName, attachment);
            msg.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_cfg["Smtp:Host"], int.Parse(_cfg["Smtp:Port"]!),
                MailKit.Security.SecureSocketOptions.StartTls);
            if (!string.IsNullOrEmpty(_cfg["Smtp:User"]))
                await smtp.AuthenticateAsync(_cfg["Smtp:User"], _cfg["Smtp:Password"]);
            await smtp.SendAsync(msg);
            await smtp.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Email send failed to {To}", to);
        }
    }

    public Task SendAdminAsync(string subject, string body)
        => SendAsync(_cfg["Smtp:AdminEmail"]!, subject, body);
}
