
using System.Net;
using System.Text;
using System.Text.Json;
using Canteen.Services.EmailServices;
using Microsoft.VisualBasic.CompilerServices;

namespace Canteen.Middlewares;

class GlobalErrorHandlerMiddleware
{
    readonly RequestDelegate next;
    readonly ILogger<GlobalErrorHandlerMiddleware> logger;
    readonly IEmailSender emailSender;
    readonly List<string> receivers;
    readonly bool isProductionEnvironment;

    public GlobalErrorHandlerMiddleware(
        RequestDelegate next,
        IHostEnvironment hostEnvironment,
        ILogger<GlobalErrorHandlerMiddleware> logger,
        IConfiguration configuration,
        IEmailSender emailSender)
        //Utils utils)
    {
        this.next = next;
        this.logger = logger;
        this.emailSender = emailSender;

        isProductionEnvironment = hostEnvironment.EnvironmentName.ToLowerInvariant() == "production";

        // receivers = utils.GetUnhandledExceptionsEmailReceivers(configuration);
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Exception");

            if (isProductionEnvironment)
                await SendMailExceptionAsync(e);

            var response = context.Response;
            response.ContentType = "application/json";
            response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var responseBody = new
            {
                ErrorCode = 500,
                ErrorMessage = "Internal server error",
            };

            var result = JsonSerializer.Serialize(responseBody);

            await response.WriteAsync(result);
        }
    }

    async Task SendMailExceptionAsync(Exception ex)
    {
        if (!receivers.Any())
            return;

        try
        {
            var messageContent = DumpExceptionMail(ex);

            await emailSender.SendEmailAsync(
                receivers,
                null,
                "Unhandled exception",
                messageContent);
        }
        catch (Exception e)
        {
            logger.LogError("[GlobalErrorHandlerMiddleware] {errorMessage}", e);
        }
    }

    static string DumpExceptionMail(Exception ex)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"<pre>");
        sb.AppendLine($"<strong>Message: [{ex.Message}]</strong>");
        sb.AppendLine($"Source: [{ex.Source}]");
        sb.AppendLine($"TargetSite: [{ex.TargetSite}]");
        sb.AppendLine($"Type: [{ex.GetType().FullName}]");
        sb.AppendLine($"HResult: [{ex.HResult}]");
        sb.AppendLine($"Data keys:");

        try
        {
            foreach (var item in ex.Data.Keys)
                sb.AppendLine($"   key: {item.ToString()}, value: {ex.Data[(string)item]}");
        }
        catch (Exception exc)
        {
            sb.AppendLine($"<strong>Exception thrown: {exc.ToString()}</strong>");
        }

        sb.AppendLine($"StackTrace: [{ex.StackTrace}]");

        if (ex.InnerException != null)
        {
            sb.AppendLine("");
            sb.AppendLine($"<strong>******* Dumping inner exception ...</strong>");
            sb.AppendLine("</pre>");
            sb.Append(DumpExceptionMail(ex.InnerException));
        }

        sb.AppendLine($"</pre>");

        return sb.ToString();
    }
}
