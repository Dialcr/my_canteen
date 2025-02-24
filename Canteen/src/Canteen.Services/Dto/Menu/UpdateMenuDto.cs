using System.Collections.Generic;

namespace Canteen.Services.Dto.Menu
{
    public class UpdateMenuDto
    {
        public int MenuId { get; set; }
        public IEnumerable<MenuProductDto> Products { get; set; } = [];
    }
}