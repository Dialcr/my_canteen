namespace Canteen.Services.Dto;

public class MenuOutputDto
{
    public int Id { get; set; }

    public DateTimeOffset Date { get; set; }

    public ICollection<MenuProductOutputDto>? MenuProducts { get; set; }
}

public static class MenuExtention
{
    public static MenuOutputDto ToMenuOutputDto(this DataAccess.Entities.Menu menu)
    {

        return new MenuOutputDto()
        {

            Id = menu.Id,
            Date = menu.Date,
            MenuProducts = menu.MenuProducts!.Select(x => x.ToEstablishmentOutputDtoWithProducts()).ToList()
        };

    }
}