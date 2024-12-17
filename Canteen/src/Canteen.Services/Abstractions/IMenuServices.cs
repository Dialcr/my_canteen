using Canteen.Services.Dto;
using Canteen.Services.Dto.Menu;
using Canteen.Services.Dto.Responses;

namespace Canteen.Services.Abstractions;

public interface IMenuServices
{
    public OneOf<ResponseErrorDto, Menu> GetMenuByEstablishmentAndDate(int idEstablishment, DateTimeOffset date);
    public OneOf<ResponseErrorDto, Response<NoContentData>> CreateMenu(CreateMenuInputDto menu);

}
