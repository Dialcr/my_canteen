using System;

namespace Canteen.Services.Dto.Menu;

public class CreateMenuInputDto
{
    public IEnumerable<MenuProductDto> MenuProducts { get; set; } = [];
    public DateTime MenuDate { get; set; }
    public int EstablishmentId { get; set; }
}

public class MenuProductDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}