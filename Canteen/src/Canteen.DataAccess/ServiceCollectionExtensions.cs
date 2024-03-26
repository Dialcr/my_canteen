using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Canteen.DataAccess;

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

}