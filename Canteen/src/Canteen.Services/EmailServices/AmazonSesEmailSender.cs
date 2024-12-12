using Amazon;
using Amazon.Runtime;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Canteen.Services.EmailServices;

public class AmazonSesEmailSender : IEmailSender
{
    readonly string fromAddress;
    readonly string displayName;
    readonly string emailAddress;
    readonly string subjectPrefix;
    readonly string accessKey;
    readonly string secretKey;
    readonly BasicAWSCredentials credentials;

    public AmazonSesEmailSender(
        IConfiguration configuration,
        IOptions<EmailSettings> emailSettings)
    {
        var settings = emailSettings.Value;
        fromAddress = settings.EmailAddress;
        displayName = settings.EmailAddressDisplay;

        emailAddress = configuration["EmailSettings:EmailAddress"];
        subjectPrefix = configuration["EmailSettings:EnvironmentSubjectPrefix"];
        accessKey = configuration["AmazonSES:AWSAccessKey"];
        secretKey = configuration["AmazonSES:AWSSecretKey"];
        credentials = new BasicAWSCredentials(accessKey, secretKey);
    }

    public async Task SendEmailAsync(List<string> receivers, List<string>? carbonCopy, string subject, string messageBody)
    {
        if (!receivers.Any())
            return;

        // Create the email client
        var emailClient = CreateEmailClient();

        // Build the destination for the email
        var destination = CreateEmailDestination(receivers);

        // Build the email message
        var emailMessage = CreateEmailMessage(subject, messageBody);

        // Prepare the email request
        var emailRequest = CreateEmailRequest(destination, emailMessage);

        // Send the email
        await SendEmailAsync(emailClient, emailRequest);
    }

    private AmazonSimpleEmailServiceClient CreateEmailClient()
    {
        return new AmazonSimpleEmailServiceClient(credentials, RegionEndpoint.USEast1);
    }

    private Destination CreateEmailDestination(List<string> receivers)
    {
        return new Destination
        {
            ToAddresses = receivers
        };
    }

    private Message CreateEmailMessage(string subject, string messageBody)
    {
        return new Message
        {
            Subject = new Content($"{subjectPrefix} {subject}"),
            Body = new Body
            {
                Html = new Content
                {
                    Charset = "UTF-8",
                    Data = messageBody
                }
            }
        };
    }

    private SendEmailRequest CreateEmailRequest(Destination destination, Message message)
    {
        return new SendEmailRequest
        {
            Source = emailAddress,
            Destination = destination,
            Message = message
        };
    }

    private async Task SendEmailAsync(AmazonSimpleEmailServiceClient client, SendEmailRequest request)
    {
        await client.SendEmailAsync(request);
    }

}
