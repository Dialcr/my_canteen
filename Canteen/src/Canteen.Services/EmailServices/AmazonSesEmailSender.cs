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

    public async Task SendEmailAsync(
        List<string> receivers,
        List<string>? carbonCopy,
        string subject,
        string messageBody)
    {
        if (!receivers.Any())
            return;

        using var client = new AmazonSimpleEmailServiceClient(credentials, RegionEndpoint.USEast1);
        var sendRequest = new SendEmailRequest
        {
            Source = emailAddress,
            Destination = new Destination
            {
                ToAddresses = receivers,
            },
            Message = new Message
            {
                Subject = new Content($"{subjectPrefix} {subject}"),
                Body = new Body
                {
                    Html = new Content
                    {
                        Charset = "UTF-8",
                        Data = messageBody,
                    },
                }
            }
        };

        var response = await client.SendEmailAsync(sendRequest);
    }
}
