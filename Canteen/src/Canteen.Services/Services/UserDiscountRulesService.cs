using Canteen.DataAccess;

namespace Canteen.Services.Services;

public class UserDiscountRulesService : CustomServiceBase
{
    public UserDiscountRulesService(EntitiesContext context) : base(context)
    {
    }
}