
using System.Text.Json.Serialization;
using Canteen.Services.Dto.User;

namespace Canteen.Services.Security;

public class AuthResponseDtoOutput
{
    public Jwt Authentication { get; set; }

    public UserOutputDto User { get; set; }

    [JsonIgnore]
    public int UserId { get; set; }
}
