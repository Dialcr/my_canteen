using Canteen.Services.Dto.Establishment;
using Canteen.Services.Services;
using Riok.Mapperly.Abstractions;

namespace Canteen.Services.Dto.Mapper;

[Mapper]
public static partial class ProductOutputDtoMapper 
{
    public static partial ProductOutputDto ToProductOutputDto(this Product source);
}