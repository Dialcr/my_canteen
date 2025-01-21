using System.Text.Json.Serialization;

namespace Canteen.Services.Security;

public class Jwt
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = "";
}
