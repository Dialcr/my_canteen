using Canteen.Services.EmailServices;
using Canteen.Services.IpAdress;
using Canteen.Services.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Canteen.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection SetOurServices(this IServiceCollection services)
    {
        
        services.AddSingleton<IEmailSender, AmazonSesEmailSender>();
        services.AddScoped<IpAddressServices, IpAddressServices>();
        
        services.AddScoped<CanteenOrderServices, CanteenOrderServices>();
        services.AddScoped<CartServices, CartServices>();
        services.AddScoped<EstablishmentService, EstablishmentService>();
        services.AddScoped<MenuServices, MenuServices>();
        services.AddScoped<ProductServices, ProductServices>();
        services.AddScoped<RequestServices, RequestServices>();
        
        return services;
    }

    
}