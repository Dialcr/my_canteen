using Canteen.DataAccess;
using Canteen.DataAccess.Enums;
using Canteen.Services.Abstractions;
using Canteen.Services.Dto;
using Canteen.Services.Dto.Menu;
using Canteen.Services.Dto.Responses;

namespace Canteen.Services.Services;

public class MenuServices(EntitiesContext context) : CustomServiceBase(context), IMenuServices
{
    public OneOf<ResponseErrorDto, Menu> GetMenuByEstablishmentAndDate(int idEstablishment, DateTimeOffset date)
    {
        var menu = FindMenuByEstablishmentAndDate(idEstablishment, date);

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
            .Where(x => x.EstablishmentId == idEstablishment && x.Date.Date >= DateTime.Now.Date);
        return menus.ToList();
    }
    private Menu? FindMenuByEstablishmentAndDate(int idEstablishment, DateTimeOffset date)
    {
        return context.Menus
            .Include(menu => menu.MenuProducts)
            .ThenInclude(menuProduct => menuProduct.Product)
            .ThenInclude(product => product!.DietaryRestrictions)
            .Include(menu => menu.MenuProducts)
            .ThenInclude(menuProduct => menuProduct.Product)
            .ThenInclude(product => product!.ImagesUrl)
            .SingleOrDefault(menu =>
                menu.EstablishmentId == idEstablishment &&
                menu.Date.Date == date.Date &&
                menu.Establishment.StatusBase == StatusBase.Active);
    }

    private OneOf<ResponseErrorDto, Menu> CreateMenuNotFoundError(int idEstablishment, DateTimeOffset date)
    {
        return Error("Menu not found",
            $"The Menu of establishment with id {idEstablishment} in the date {date} has not found",
            400);
    }
    public OneOf<ResponseErrorDto, Response<NoContentData>> CreateMenu(CreateMenuInputDto newMenu)
    {
        var existingMenu = context.Menus.Where(x => x.Date.Date == newMenu.MenuDate.Date);
        if (existingMenu.Any())
        {
            return Error("Menu already exists", "Menu already exists in that date ", 400);
        }
        var establishment = context.Establishments.FirstOrDefault(x => x.Id == newMenu.EstablishmentId && x.StatusBase == StatusBase.Active);
        if (establishment is null)
        {
            return Error("Establishment not found", "Establishment not found", 400);
        }
        var productsIds = newMenu.MenuProducts.Select(x => x.ProductId);
        var products = context.Products.Where(x => productsIds.Contains(x.Id) && x.EstablishmentId == newMenu.EstablishmentId)
        .Include(x => x.EstablishmentId);
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

        return new Response<NoContentData>();
    }

}