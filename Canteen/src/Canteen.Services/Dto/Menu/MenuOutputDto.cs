using Canteen.Services.Dto.DeliveryTime;

namespace Canteen.Services.Dto;

public class MenuOutputDto
{
    public int Id { get; set; }

    public DateTimeOffset Date { get; set; }

    public IEnumerable<MenuProductOutputDto>? MenuProducts { get; set; }
    public IEnumerable<DeliveryTimeOutputDto> DeliveryTimes { get; set; } = [];
    public string StatusBase { get; set; } = string.Empty;



}

public static class MenuExtention
{
    public static MenuOutputDto ToMenuOutputDto(this DataAccess.Entities.Menu menu)
    {

        return new MenuOutputDto()
        {

            Id = menu.Id,
            Date = menu.Date,
            MenuProducts = menu.MenuProducts!.Select(x => x.ToEstablishmentOutputDtoWithProducts()).ToList(),
            DeliveryTimes = (menu.Establishment.DeliveryTimes is not null)
                ? menu.Establishment.DeliveryTimes.Select(x => x.ToDeliveryTimeOutputDto())
                : [],
            StatusBase = menu.StatusBase.ToString()
        };

    }
}