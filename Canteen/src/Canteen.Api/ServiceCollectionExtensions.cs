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
    
    public static IServiceCollection SetDbContext(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        
        var serverVersion = new MySqlServerVersion(new Version(8, 0, 33));
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        services.AddDataProtection().PersistKeysToDbContext<EntitiesContext>();
        services.AddDbContext<DbContext, EntitiesContext>(options => options
            .UseMySql(connectionString, serverVersion)
            .EnableSensitiveDataLogging(false)
            .EnableDetailedErrors()
        );
    
        return services;
    }
    
    
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
    public static IServiceCollection SetOurServices(this IServiceCollection services)
    {
        
        services.AddSingleton<IEmailSender, AmazonSesEmailSender>();
        
        services.AddScoped<CanteenOrderServices, CanteenOrderServices>();
        services.AddScoped<EstablishmentService, EstablishmentService>();
        services.AddScoped<MenuServices, MenuServices>();
        services.AddScoped<ProductServices, ProductServices>();
        services.AddScoped<RequestServices, RequestServices>();
        services.AddScoped<IpAddressServices, IpAddressServices>();
        
        return services;
    }
    
    //todo: revisar si estas configuraciones hacen falta
    public static IServiceCollection SetSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(option =>
        {
            option.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "CompraTodayAPI",
                Version = "1",
            });

            option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });

            option.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type=ReferenceType.SecurityScheme,
                            Id="Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    
}