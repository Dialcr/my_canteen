using Canteen.DataAccess;
using Canteen.Services.EmailServices;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Canteen.Services;
using Canteen.Services.IpAdress;
using Canteen.Services.Services;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Canteen.DataAccess.Entities;

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
            // options.AddPolicy(name: policyName, builder =>
            // {
            //     builder
            //         .WithOrigins(origins)
            //         .AllowAnyMethod()
            //         .AllowAnyHeader()
            //         .AllowCredentials();
            // });
            options.AddDefaultPolicy(policy =>
            {
                policy.AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin();
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

    public static IServiceCollection SetAuthentication(
            this IServiceCollection services,
            IConfiguration configuration
        )
    {
        var a = configuration
            .GetSection("Authentication")
            .GetSection("Password")
            .GetValue<int>("RequiredLength");
        services
            .AddIdentity<AppUser, IdentityRole<int>>(options =>
            {
                options.Password.RequiredLength = configuration
                    .GetSection("Authentication")
                    .GetSection("Password")
                    .GetValue<int>("RequiredLength");
                options.Password.RequireNonAlphanumeric = configuration
                    .GetSection("Authentication")
                    .GetSection("Password")
                    .GetValue<bool>("RequireNonAlphanumeric");
                options.Password.RequireDigit = configuration
                    .GetSection("Authentication")
                    .GetSection("Password")
                    .GetValue<bool>("RequireDigit");
                options.Password.RequireLowercase = configuration
                    .GetSection("Authentication")
                    .GetSection("Password")
                    .GetValue<bool>("RequireLowercase");
                options.Password.RequireUppercase = configuration
                    .GetSection("Authentication")
                    .GetSection("Password")
                    .GetValue<bool>("RequireUppercase");

                options.ClaimsIdentity = new ClaimsIdentityOptions
                {
                    EmailClaimType = ClaimTypes.Email,
                    RoleClaimType = ClaimTypes.Role,
                    UserIdClaimType = ClaimTypes.NameIdentifier,
                    UserNameClaimType = ClaimTypes.Name
                };
                //change this emailconfirm
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<EntitiesContext>()
            .AddDefaultTokenProviders()
            .AddApiEndpoints();

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,

                    ValidAudience = configuration["JwtSettings:Audience"],
                    ValidIssuer = configuration["JwtSettings:Issuer"],
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"])
                    ),
                };
            });

        return services;
    }

    public static IServiceCollection SetSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(option =>
        {
            option.SwaggerDoc("v1", new OpenApiInfo { Title = "Practices", Version = "1", });

            option.AddSecurityDefinition(
                "Bearer",
                new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                }
            );

            option.AddSecurityRequirement(
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                }
            );
        });

        return services;
    }
}