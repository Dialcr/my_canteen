using Canteen.DataAccess;

namespace Canteen.Services;

public class CustomServiceBase
{
    protected readonly EntitiesContext context;

    public CustomServiceBase(EntitiesContext context)
    {
        this.context = context;
    }
}
