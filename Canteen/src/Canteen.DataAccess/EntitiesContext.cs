using Canteen.DataAccess.Entities;
using Canteen.DataAccess.Enums;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Canteen.DataAccess;

public class EntitiesContext : IdentityDbContext<AppUser, IdentityRole<int>, int> //, IDataProtectionKeyContext
{
    public EntitiesContext(DbContextOptions<EntitiesContext> options) : base(options)
    {

    }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Establishment> Establishments { get; set; }
    public DbSet<EstablishmentCategory> EstablishmentsCategory { get; set; }
    public DbSet<CanteenRequest> Requests { get; set; }
    public DbSet<RequestProduct> RequestProducts { get; set; }
    public DbSet<Menu> Menus { get; set; }
    public DbSet<MenuProduct> MenuProducts { get; set; }

    public DbSet<DataProtectionKey> DataProtectionKeys { get; }
    public DbSet<ProductImageUrl> ProductImageUrls { get; set; }
    public DbSet<Discount> Discounts { get; set; }
    public DbSet<KeyValueData> KeyValueData { get; set; }
    public DbSet<DietaryRestriction> DietaryRestrictions { get; set; }

    public DbSet<CanteenCart> Carts { get; set; }
    public DbSet<DeliveryTime> DeliveryTimes { get; set; }
    public DbSet<AppUser> Users { get; set; }
    public DbSet<IdentityRole> IdentityRole { get; set; }
    public DbSet<OrderOwner> OrderOwners { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ConfigureOrderEntity(modelBuilder);
        ConfigureEstablishmentEntity(modelBuilder);
        ConfigureMenuEntity(modelBuilder);
        ConfigureMenuProductEntity(modelBuilder);
        ConfigureProductEntity(modelBuilder);
        ConfigureDiscountEntity(modelBuilder);
        ConfigureCanteenCartEntity(modelBuilder);
        ConfigureOrderIdentity(modelBuilder);

    }

    private void ConfigureOrderIdentity(ModelBuilder modelBuilder)
    {
        // modelBuilder.Entity<IdentityUserLogin<int>>()
        //     .HasKey(l => new { l.LoginProvider, l.ProviderKey, l.UserId });

        modelBuilder
            .Entity<IdentityRole<int>>()
            .HasData(
                new IdentityRole<int>[]
                {
                    new IdentityRole<int>
                    {
                        Id = 1,
                        Name = RoleNames.ADMIN,
                        NormalizedName = RoleNames.ADMIN.Trim().ToUpper().Replace(" ", ""),
                    },
                    new IdentityRole<int>
                    {
                        Id = 2,
                        Name = RoleNames.CLIENT,
                        NormalizedName = RoleNames.CLIENT.Trim().ToUpper().Replace(" ", ""),
                    }
                }
            );
    }

    private void ConfigureOrderEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>()
            .HasMany(x => x.Requests)
            .WithOne(x => x.Order);

        modelBuilder.Entity<Order>()
            .HasOne(x => x.Establishment)
            .WithMany(x => x.Orders);

        modelBuilder.Entity<Order>()
            .HasOne(x => x.Owner)
            .WithOne();
    }

    private void ConfigureEstablishmentEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Establishment>()
            .HasMany(x => x.Products)
            .WithOne(x => x.Establishment);

        modelBuilder.Entity<Establishment>()
        .HasMany(x => x.DeliveryTimes)
        .WithOne(x => x.Establishment);

        modelBuilder.Entity<Establishment>()
        .HasMany(x => x.EstablishmentCategories)
        .WithMany(x => x.Establishments);
    }

    private void ConfigureMenuEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Menu>()
            .HasMany(x => x.MenuProducts)
            .WithOne(x => x.Menu);

        modelBuilder.Entity<Menu>()
            .HasOne(x => x.Establishment)
            .WithMany(x => x.Menus);
    }

    private void ConfigureMenuProductEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MenuProduct>()
            .HasOne(x => x.Product);
    }

    private void ConfigureProductEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>()
            .HasMany(x => x.DietaryRestrictions)
            .WithMany(x => x.Products);

        modelBuilder.Entity<Product>()
            .HasMany(x => x.ImagesUrl)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId);
    }

    private void ConfigureDiscountEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Discount>()
            .HasOne(x => x.Establishment)
            .WithMany(x => x.Discounts);
    }

    private void ConfigureCanteenCartEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CanteenCart>()
            .HasMany(x => x.Requests);

        modelBuilder.Entity<CanteenCart>()
        .HasOne(x => x.Establishment);

        modelBuilder.Entity<CanteenCart>()
        .HasOne(x => x.User);
    }

}