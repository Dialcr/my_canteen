using Canteen.DataAccess;
using Canteen.Services.Dto;

namespace Canteen.Services.Services;

public class CustomServiceBase(EntitiesContext context)
{
    protected ResponseErrorDto Error(string title, string details, int status)
        => new()
        {
            Status = status,
            Detail = details,
            Title = title
        };
}
