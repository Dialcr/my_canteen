using Canteen.DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace Canteen.DataAccess;

public class EntitiesContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<DayProduct> DayProducts { get; set; }
    public DbSet<Establishment> Establishments { get; set; }
    public DbSet<Menu> Menus { get; set; }
    public DbSet<Request> Requests { get; set; }
}