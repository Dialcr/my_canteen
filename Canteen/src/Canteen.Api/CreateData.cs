using System;
using Canteen.DataAccess;
using Canteen.DataAccess.Entities;
using Canteen.DataAccess.Enums;
using Microsoft.AspNetCore.Identity;

namespace Canteen;

public static class CreateData
{
    public static async Task AddDataIntoDatabaseAsync(IServiceProvider services, IConfiguration configuration)
    {
        using (var scopeDoWork = services.CreateScope())
        {
            var dbContext = scopeDoWork.ServiceProvider.GetRequiredService<EntitiesContext>();
            var userManager = scopeDoWork.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            if (!dbContext.Users.Any())
            {
                var establishment = new Establishment
                {
                    Name = "Canteen Demo",
                    Description = "Canteen Demo",
                    DeliveryTimes = new List<DeliveryTime>
                    {
                        new DeliveryTime
                        {
                            StartTime = new TimeSpan(7, 0, 0),
                            EndTime = new TimeSpan(9, 0, 0),
                            DeliveryTimeType = DeliveryTimeType.Breakfast,
                            EstablishmentId = 1
                        },
                        new DeliveryTime
                        {
                            StartTime = new TimeSpan(12, 0, 0),
                            EndTime = new TimeSpan(14, 0, 0),
                            DeliveryTimeType = DeliveryTimeType.Lunch,
                            EstablishmentId = 1
                        }
                    },
                    Products = new List<Product>{
                        new Product {
                        Name = "Hamburger",
                        Description = "Classic beef hamburger",
                        Category = ProductCategory.Dessert,
                        Price = 8.99M,
                        Ingredients = "Beef patty, lettuce, tomato, cheese, bun",
                        DietaryRestrictions = [],
                        ImagesUrl = []
                    },
                    new Product {
                        Name = "Pizza",
                        Description = "Traditional Italian pizza",
                        Category = ProductCategory.Starter,
                        Price = 10.99M,
                        EstablishmentId = 1,
                        Ingredients = "Pizza dough, tomato sauce, cheese, pepperoni",
                        DietaryRestrictions = [],
                        ImagesUrl = []
                    },
                    new Product {
                        Name = "Salad",
                        Description = "Fresh garden salad",
                        Category = ProductCategory.MainCourse,
                        Price = 6.99M,
                        Ingredients = "Lettuce, tomatoes, cucumbers, carrots, dressing",
                        DietaryRestrictions = [],
                        ImagesUrl = []
                    },
                    new Product {
                        Name = "Soda",
                        Description = "Carbonated soft drink",
                        Category = ProductCategory.MainCourse,
                        Price = 2.99M,
                        Ingredients = "Carbonated water, sugar, natural flavors",
                        DietaryRestrictions = [],
                        ImagesUrl = []
                    }
                    }
                };

                dbContext.Establishments.Add(establishment);
                dbContext.SaveChanges();

                var establisments = dbContext.Establishments.Where(e => e.Name == "Canteen Demo").ToList();
                var establismentFind = establisments.Last();
                var startDate = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek + 1);

                var products = dbContext.Products.Where(x => x.EstablishmentId == establismentFind.Id).ToList();
                var menus = new List<Menu>();

                for (int i = 0; i < 5; i++)
                {
                    var menu = new Menu
                    {
                        Date = startDate.AddDays(i).ToUniversalTime(),
                        EstablishmentId = establismentFind.Id,
                        MenuProducts = new List<MenuProduct>
                          {
                              new MenuProduct
                              {
                                  CanteenProductId = products[i % products.Count].Id,
                                  Quantity = 10
                              }
                          }
                    };
                    menus.Add(menu);
                }

                dbContext.Menus.AddRange(menus);
                dbContext.SaveChanges();

                var superAdminName = configuration["SuperAdminData:Name"];
                var superAdminEmail = configuration["SuperAdminData:Email"];
                var superAdminPassword = configuration["SuperAdminData:Password"];

                var result = await userManager!.CreateAsync(
                    new AppUser()
                    {
                        Email = superAdminEmail,
                        UserName = superAdminName,
                        EstablishmentId = establismentFind.Id,
                    },
                    superAdminPassword!
                );
                var user = await userManager.FindByNameAsync(superAdminName);

                await userManager.AddToRoleAsync(user, RoleNames.Admin.ToUpper());

            }
        }
    }
}
