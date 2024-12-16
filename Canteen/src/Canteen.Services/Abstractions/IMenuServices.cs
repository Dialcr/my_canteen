using System;
using Canteen.Services.Dto;

namespace Canteen.Services.Abstractions;

public interface IMenuServices
{
    OneOf<ResponseErrorDto, Menu> GetMenuByEstablishmentAndDate(int idEstablishment, DateTimeOffset date);

}
