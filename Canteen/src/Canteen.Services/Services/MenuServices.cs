using Canteen.DataAccess;
using Canteen.DataAccess.Enums;
using Canteen.Services.Abstractions;
using Canteen.Services.Dto;
using Canteen.Services.Dto.Menu;
using Canteen.Services.Dto.Responses;

namespace Canteen.Services.Services;

public class MenuServices(EntitiesContext context) : CustomServiceBase(context), IMenuServices
{
    public OneOf<ResponseErrorDto, Menu> GetMenuByEstablishmentAndDate(int idEstablishment, DateTimeOffset date, bool useInactive = false)
    {
        var menu = context!.Menus!
            .Include(menu => menu.MenuProducts)!
                .ThenInclude(menuProduct => menuProduct.Product)
                    .ThenInclude(product => product!.DietaryRestrictions)
            .Include(menu => menu.MenuProducts)!
                .ThenInclude(menuProduct => menuProduct.Product)
                    .ThenInclude(product => product!.ImagesUrl)
            .FirstOrDefault(menu =>
                menu.EstablishmentId == idEstablishment &&
                menu.Date.Date == date.Date &&
                (useInactive == false || menu.Establishment!.StatusBase == StatusBase.Active) &&
                (useInactive == false || menu.StatusBase == StatusBase.Active));

        if (menu is null)
        {
            return CreateMenuNotFoundError(idEstablishment, date);
        }

        return menu;
    }
    public OneOf<ResponseErrorDto, IEnumerable<Menu>> ListMenuByEstablishment(int idEstablishment)
    {
        var menus = context.Menus
            .Include(menu => menu.MenuProducts)
                .ThenInclude(menuProduct => menuProduct.Product)
                    .ThenInclude(product => product!.DietaryRestrictions)
            .Include(menu => menu.MenuProducts)
                .ThenInclude(menuProduct => menuProduct.Product)
                    .ThenInclude(product => product!.ImagesUrl)
            .Include(x => x.Establishment)
                .ThenInclude(x => x.DeliveryTimes)
            .Where(x => x.EstablishmentId == idEstablishment)
            .AsEnumerable()
            .Where(x => x.Date.Date >= DateTime.Now.Date);
        return menus.ToList();
    }
    private OneOf<ResponseErrorDto, Menu> CreateMenuNotFoundError(int idEstablishment, DateTimeOffset date)
    {
        return Error("Menu not found",
            $"The Menu of establishment with id {idEstablishment} in the date {date} has not found",
            400);
    }
    public OneOf<ResponseErrorDto, Response<NoContentData>> CreateMenu(CreateMenuInputDto newMenu)
    {
        var existingMenu = context.Menus.Where(x => x.EstablishmentId == newMenu.EstablishmentId).AsEnumerable().Where(x => x.Date.Date == newMenu.MenuDate.Date);
        if (existingMenu.Any())
        {
            return Error("Menu already exists", "Menu already exists in that date ", 400);
        }
        var establishment = context.Establishments.FirstOrDefault(x => x.Id == newMenu.EstablishmentId && x.StatusBase == StatusBase.Active);
        if (establishment is null)
        {
            return Error("Establishment not found", "Establishment not found", 400);
        }
        var productsIds = newMenu.MenuProducts.Select(x => x.ProductId).ToList();
        var products = context.Products.Where(x => productsIds.Contains(x.Id) && x.EstablishmentId == newMenu.EstablishmentId)
        .Include(x => x.Establishment).ToList();
        if (products.Count() != productsIds.Count())
        {
            return Error("Some products no found from that establishment", "Some products no found from that establishment", 400);
        }
        var menu = new Menu()
        {
            Date = newMenu.MenuDate,
            EstablishmentId = newMenu.EstablishmentId,
            MenuProducts = newMenu.MenuProducts.Select(x => new Canteen.DataAccess.Entities.MenuProduct()
            {
                CanteenProductId = x.ProductId,
                Quantity = x.Quantity,
            }).ToList()
        };
        context.Menus.Add(menu);
        context.SaveChanges();
        return new Response<NoContentData>();
    }

    public OneOf<ResponseErrorDto, Response<NoContentData>> ChangeMenuStatus(int menuId, StatusBase newStatus)
    {
        var menu = context.Menus.Find(menuId);
        if (menu == null)
        {
            return Error("Menu not found", $"Menu with ID {menuId} not found", 400);
        }

        menu.StatusBase = newStatus;
        context.SaveChanges();
        return new Response<NoContentData>();
    }

    public OneOf<ResponseErrorDto, Response<NoContentData>> ToggleMenuStatus(int menuId)
    {
        var menu = context.Menus.Find(menuId);
        if (menu == null)
        {
            return Error("Menu not found", $"Menu with ID {menuId} not found", 400);
        }

        menu.StatusBase = menu.StatusBase == StatusBase.Active ? StatusBase.Inactive : StatusBase.Active;
        context.SaveChanges();
        return new Response<NoContentData>();
    }

    public OneOf<ResponseErrorDto, Response<NoContentData>> UpdateMenuProducts(int menuId, IEnumerable<MenuProductDto> products)
    {
        var menu = context.Menus
            .Include(m => m.MenuProducts)
            .FirstOrDefault(m => m.Id == menuId);

        if (menu == null)
        {
            return Error("Menu not found", $"Menu with ID {menuId} not found", 400);
        }

        var productIds = products.Select(p => p.ProductId).ToList();
        var existingProducts = menu.MenuProducts.Select(mp => mp.CanteenProductId).ToList();

        // Add new products or update quantity of existing products
        foreach (var product in products)
        {
            var existingProduct = menu.MenuProducts.FirstOrDefault(mp => mp.CanteenProductId == product.ProductId);
            if (existingProduct != null)
            {
                existingProduct.Quantity = product.Quantity;
            }
            else
            {
                menu.MenuProducts.Add(new MenuProduct
                {
                    CanteenProductId = product.ProductId,
                    Quantity = product.Quantity,
                    MenuId = menuId
                });
            }
        }

        // Remove products not in the new list
        var productsToRemove = menu.MenuProducts.Where(mp => !productIds.Contains(mp.CanteenProductId)).ToList();
        foreach (var productToRemove in productsToRemove)
        {
            menu.MenuProducts.Remove(productToRemove);
        }

        context.SaveChanges();
        return new Response<NoContentData>();
    }

}