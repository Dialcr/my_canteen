using Canteen.DataAccess;
using Canteen.Services.Dto;

namespace Canteen.Services.Services;

public class MenuServices(EntitiesContext context) : CustomServiceBase(context)
{
    public OneOf<ResponseErrorDto, Menu> GetMenuByEstablishmentAndDate(int idEstablishment, DateTimeOffset date)
    {
        var menu = FindMenuByEstablishmentAndDate(idEstablishment, date);

        if (IsMenuNotFound(menu))
        {
            return CreateMenuNotFoundError(idEstablishment, date);
        }

        return menu;
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
                menu.Date.Date == date.Date);
    }

    private bool IsMenuNotFound(Menu? menu)
    {
        return menu is null;
    }

    private OneOf<ResponseErrorDto, Menu> CreateMenuNotFoundError(int idEstablishment, DateTimeOffset date)
    {
        return Error("Menu not found",
            $"The Menu of establishment with id {idEstablishment} in the date {date} has not found",
            400);
    }

}