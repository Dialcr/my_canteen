using Canteen.DataAccess.Entities;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Canteen.DataAccess;

public class EntitiesContext : DbContext, IDataProtectionKeyContext
{
    public EntitiesContext(DbContextOptions<EntitiesContext> options) : base(options)
    {
        
    }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Establishment> Establishments { get; set; }
    public DbSet<Request> Requests { get; set; }
    public DbSet<RequestProduct> RequestProducts { get; set; }
    public DbSet<Menu> Menus { get; set; }
    public DbSet<MenuProduct> MenuProducts { get; set; }
    
    public DbSet<DataProtectionKey> DataProtectionKeys { get; }
    public DbSet<ProductImageUrl> ProductImageUrls { get; set; }
    public DbSet<Discount> Discounts { get; set; }
    public DbSet<KeyValueData> KeyValueData { get; set; }
    public DbSet<KeyValueData> DietaryRestrictions { get; set; }

    public DbSet<CanteenCart> Carts { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>().
            HasMany(x => x.Requests)
            .WithOne(x=>x.Order);
        
        modelBuilder.Entity<Order>()
            .HasOne(x=>x.Establishment)
            .WithMany(x=>x.Orders);
        
        modelBuilder.Entity<Establishment>()
            .HasMany(x => x.Products)
            .WithOne(x => x.Establishment);

        modelBuilder.Entity<Menu>()
            .HasMany(x => x.MenuProducts)
            .WithOne(x => x.Menu);
        
        modelBuilder.Entity<Menu>()
            .HasOne(x =>x.Establishment)
            .WithMany(x=>x.Menus);
        
        modelBuilder.Entity<MenuProduct>()
            .HasOne(x => x.Product);
        modelBuilder.Entity<Product>()
            .HasMany(x=>x.DietaryRestrictions)
            .WithMany(x=>x.Products);

        modelBuilder.Entity<Product>()
            .HasMany(x => x.ImagesUrl);
        
        modelBuilder.Entity<Discount>()
            .HasOne(x=>x.Establishment)
            .WithMany(x=>x.Discounts);
        
        modelBuilder.Entity<CanteenCart>()
            .HasMany(x=>x.Requests);
    }
}