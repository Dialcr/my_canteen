using Canteen.DataAccess.Enums;
using Canteen.Services.Dto;
using Canteen.Services.Dto.Menu;
using Canteen.Services.Dto.Responses;

namespace Canteen.Services.Abstractions;

public interface IMenuServices
{
    public OneOf<ResponseErrorDto, Menu> GetMenuByEstablishmentAndDate(int idEstablishment, DateTimeOffset date, bool useInactive = false);
    public OneOf<ResponseErrorDto, IEnumerable<Menu>> ListMenuByEstablishment(int idEstablishment);

    public OneOf<ResponseErrorDto, Response<NoContentData>> CreateMenu(CreateMenuInputDto menu);

    OneOf<ResponseErrorDto, Response<NoContentData>> ChangeMenuStatus(int menuId, StatusBase newStatus);

    OneOf<ResponseErrorDto, Response<NoContentData>> ToggleMenuStatus(int menuId);

    OneOf<ResponseErrorDto, Response<NoContentData>> UpdateMenuProducts(int menuId, IEnumerable<MenuProductDto> products);
}
