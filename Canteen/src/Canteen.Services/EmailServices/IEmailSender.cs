namespace Canteen.Services.EmailServices;

public interface IEmailSender
{
    Task SendEmailAsync(
        List<string> receivers,
        List<string>? carbonCopy,
        string subject,
        string messageBody);
}
