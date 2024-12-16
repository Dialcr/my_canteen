using System;

namespace Canteen.Services.Abstractions;

public interface IEstablishmentService
{
    IEnumerable<Dto.Establishment.EstablishmentOutputDto> GetAllEstablishments();
    Task<OneOf<Dto.ResponseErrorDto, Dto.Establishment.EstablishmentOutputDto>> GetEstablishmentByIdAsync(int id);

}
