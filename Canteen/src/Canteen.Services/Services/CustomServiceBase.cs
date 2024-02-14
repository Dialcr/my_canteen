using Canteen.DataAccess;

namespace Canteen.Services.Services;

public class CustomServiceBase
{
    protected readonly EntitiesContext _context;

    public CustomServiceBase(EntitiesContext context)
    {
        this._context = context;
    }
}
