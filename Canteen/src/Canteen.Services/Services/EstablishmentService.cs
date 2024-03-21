using Canteen.DataAccess;
using Canteen.Services.Dto;

namespace Canteen.Services.Services;

public class EstablishmentService(EntitiesContext context) : CustomServiceBase(context)
{
    public IEnumerable<EstablishmentOutputDto> GetAllEstablishments()
    {
        var allEstablishment = _context.Establishments.Select(x=>x.ToEstablishmentOutputDto());
        return allEstablishment;
    }

    public async Task<OneOf<ResponseErrorDto, EstablishmentOutputDto>> GetEstablishmentByIdAsync(int id)
    {
        var establish = await _context.Establishments
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();

        if (establish is null)
        {
            return new ResponseErrorDto
            {
                Status = 400,
                Title = "Establishment not found",
                Detail = "Establishment not found"
            };
        }

        return establish.ToEstablishmentOutputDto();
    }
}