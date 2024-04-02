using Riok.Mapperly.Abstractions;

namespace Canteen.Services.Dto.Establishment.Mapper;

[Mapper]
public static partial class EstablishmentDtoMapper
{
    public static partial EstablishmentOutputDto ToEstablishmentOutputDto(this DataAccess.Entities.Establishment source);
}