using Canteen.DataAccess.Settings;
using Canteen.Services.Abstractions;
using Canteen.Services.EmailServices;
using Canteen.Services.IpAdress;
using Canteen.Services.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Canteen.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection SetOurServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        services.AddSingleton<IEmailSender, AmazonSesEmailSender>();
        services.AddScoped<IpAddressServices, IpAddressServices>();

        services.AddScoped<IMenuServices, MenuServices>();
        services.AddScoped<ICanteenOrderServices, CanteenOrderServices>();
        services.AddScoped<ICartServices, CartServices>();
        services.AddScoped<IEstablishmentService, EstablishmentService>();
        services.AddScoped<IProductServices, ProductServices>();
        services.AddScoped<IRequestServices, RequestServices>();
        services.AddScoped<IUserServices, UserServicers>();
        services.AddScoped<TokenUtil, TokenUtil>();
        services.AddScoped<IEstablishmentCategoryServices, EstablishmentCategoryServices>();

        return services;
    }


}