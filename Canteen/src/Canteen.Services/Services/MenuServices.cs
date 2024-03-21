using Canteen.DataAccess;
using Canteen.Services.Dto;

namespace Canteen.Services.Services;

public class MenuServices(EntitiesContext context) : CustomServiceBase(context)
{
    public OneOf<ResponseErrorDto, Menu>  GetMenuByEstablishmentAndDate(
        int idEstablishment,
        DateTimeOffset date)
    {
        var result = _context.Menus.Include(x=>x.MenuProducts)
            .ThenInclude(x=>x.Product)
            .ThenInclude(y=>y!.DietaryRestrictions)
            .Include(x=>x.MenuProducts)
            .ThenInclude(x=>x.Product)
            .ThenInclude(y=>y!.ImagesUrl)
            .SingleOrDefault(x =>
                x.EstablishmentId == idEstablishment &&
                x.Date.Date == date.Date);

        if (result is null)
        {
            return new ResponseErrorDto()
            {
                Status = 400,
                Title = "Menu not found",
                Detail = $"The Menu of establishment with id {idEstablishment} in the date {date}  has not found"
            };
        }

        return result;
    }
}