using Canteen.DataAccess;
using Canteen.Services.Dto;

namespace Canteen.Services.Services;

public class EstablishmentService : CustomServiceBase
{
    public EstablishmentService(EntitiesContext context)
        : base(context)
    {
    }

    public async Task<OneOf<ResponseErrorDto, List<Establishment>>> GetAllEstablishmentsAsync()
    {
        var allStablish = _context.Establishments.ToList();

        if (allStablish is null)
        {
            return new ResponseErrorDto
            {
                Status = 404,
                Title = "There are no Establishment",
                Detail = "There are no Establishment to obtain"
            };
        }

        return allStablish;
    }

    public async Task<OneOf<ResponseErrorDto, Establishment>> GetEstablishmentById(int id)
    {
        var establish = await _context.Establishments
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();

        if (establish is null)
        {
            return new ResponseErrorDto
            {
                Status = 404,
                Title = "Establishment not found",
                Detail = "Establishment not found"
            };
        }

        return establish;
    }
}