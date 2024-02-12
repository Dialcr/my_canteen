using Canteen.DataAccess;

namespace Canteen.Services;

public class MenuServices : CustomServiceBase
{
    public MenuServices(EntitiesContext context)
        : base(context)
    {
    }

    public OneOf<ResponseErrorDto, Menu> GetMenuByEstablishmentAndDate(
        int idEstablishment,
        DateTime date)
    {
        var result = _context.Menus
            .SingleOrDefault(x =>
                x.IdEstablishment == idEstablishment &&
                x.Date == date);

        if (result is null)
        {
            return new ResponseErrorDto()
            {
                Status = 404,

                Title = "Menu not found",
                Detail = $"The Menu of establishment with id {idEstablishment} in the date {date}  has not found"
            };
        }

        return result;
    }
}