using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Canteen.Services.EmailServices;

public class DotNetSmtpClientEmailSender : IEmailSender
{
    readonly ILogger<DotNetSmtpClientEmailSender> logger;
    readonly SmtpClient smtpClient;
    readonly string fromAddress;
    readonly string displayName;
    MailMessage? message;

    public DotNetSmtpClientEmailSender(
        IOptions<EmailSettings> emailSettings,
        ILogger<DotNetSmtpClientEmailSender> logger)
    {
        this.logger = logger;
        var settings = emailSettings.Value;
        fromAddress = settings.EmailAddress;
        displayName = settings.EmailAddressDisplay;

        smtpClient = new SmtpClient
        {
            Host = settings.SmtpServerAddress,
            Port = settings.SmtpServerPort,
            EnableSsl = settings.EnableSSL,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(settings.SmtpUserName, settings.SmtpPassword)
        };
    }

    public async Task SendEmailAsync(
        List<string> receivers,
        List<string>? carbonCopy,
        string subject,
        string messageBody)
    {
        using var message = GetMessage(receivers, carbonCopy, subject, messageBody);

        await smtpClient.SendMailAsync(message);
    }

    MailMessage GetMessage(
        IEnumerable<string> receivers,
        IEnumerable<string>? carbonCopy,
        string subject,
        string messageBody)
    {
        MailMessage message = new()
        {
            From = new MailAddress(fromAddress, displayName),
            Subject = subject,
            Body = messageBody,
            IsBodyHtml = true,
        };

        foreach (var item in receivers)
            message.To.Add(item);

        if (carbonCopy is not null)
            foreach (var item in carbonCopy)
                message.CC.Add(item);

        return message;
    }
}
