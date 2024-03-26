using Canteen.DataAccess;
using Canteen.Services.EmailServices;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Canteen.Services;
using Canteen.Services.IpAdress;
using Canteen.Services.Services;

namespace Canteen;

public static class ServiceCollectionExtensions
{
    
   
    
    public static IServiceCollection SetCors(
        this IServiceCollection services,
        IConfiguration configuration,
        string policyName)
    {
        var origins = configuration
            .GetSection("CorsAllowedOrigins")
            .Get<string[]>();

        if (origins is null)
            throw new NullReferenceException(nameof(origins));

        services.AddCors(options =>
        {
            options.AddPolicy(name: policyName, builder =>
            {
                builder
                    .WithOrigins(origins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        return services;
    }

    public static IServiceCollection SetSettings(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.Configure<OrderEmailSettings>(configuration.GetSection("OrderEmail"));
        //services.Configure<StoreSettings>(configuration.GetSection("Store"));
        
        /*
         services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection("Authentication:Jwt"))
            .ValidateDataAnnotations()
            .ValidateOnStart();
         */

        return services;
    }
    
    

    
}