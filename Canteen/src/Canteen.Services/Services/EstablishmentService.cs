using Canteen.DataAccess;
using Canteen.Services.Dto;

namespace Canteen.Services.Services;

public class EstablishmentService : CustomServiceBase
{
    public EstablishmentService(EntitiesContext context)
        : base(context)
    {
    }

    public OneOf<ResponseErrorDto, ICollection<EstablishmentOutputDto>> GetAllEstablishmentsAsync()
    {
        var allStablish = _context.Establishments;

        if (!allStablish.Any())
        {
            return new ResponseErrorDto
            {
                Status = 404,
                Title = "There are no Establishment",
                Detail = "There are no Establishment to obtain"
            };
        }

        var establismentList = allStablish.Select(x=>x.ToEstablishmentOutputDto());
        return establismentList.ToList();
    }

    public async Task<OneOf<ResponseErrorDto, EstablishmentOutputDto>> GetEstablishmentById(int id)
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

        return establish.ToEstablishmentOutputDto();
    }
    public OneOf<ResponseErrorDto, ICollection<EstablishmentOutputDto>> GetAllEstablishment()
    {
        var establish = _context.Establishments;
            
        if (!establish.Any())
        {
            return new ResponseErrorDto
            {
                Status = 404,
                Title = "Have not any establishment",
                Detail = "Have not any establishment"
            };
        }
        var establismentList = establish.Select(x=>x.ToEstablishmentOutputDto());
        return establismentList.ToList();
    }
}