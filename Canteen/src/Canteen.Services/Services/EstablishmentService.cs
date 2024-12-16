using Canteen.DataAccess;
using Canteen.Services.Abstractions;
using Canteen.Services.Dto;
using Canteen.Services.Dto.Establishment;
using Canteen.Services.Dto.Establishment.Mapper;

namespace Canteen.Services.Services;

public class EstablishmentService(EntitiesContext context) : CustomServiceBase(context), IEstablishmentService
{
    public IEnumerable<EstablishmentOutputDto> GetAllEstablishments()
    {
        var allEstablishment = context.Establishments.Select(x => x.ToEstablishmentOutputDto());
        return allEstablishment;
    }

    public async Task<OneOf<ResponseErrorDto, EstablishmentOutputDto>> GetEstablishmentByIdAsync(int id)
    {
        var establish = await context.Establishments
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();

        if (establish is null)
        {
            return Error("Establishment not found",
                "Establishment not found",
                400);
        }

        return establish.ToEstablishmentOutputDto();
    }
}