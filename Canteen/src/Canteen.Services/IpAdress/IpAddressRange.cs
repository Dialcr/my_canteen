using System.Net;
using System.Text.Json.Serialization;

namespace Canteen.Services.IpAdress;

public class IpAddressRange
{
    [JsonPropertyName("start")]
    public string IpStart { get; set; } = "";

    [JsonPropertyName("end")]
    public string IpEnd { get; set; } = "";

    public bool IsIpInRange(string ipAddress)
    {
        try
        {
            var ipToCheck = IPAddress.Parse(ipAddress);
            var ipStart = IPAddress.Parse(IpStart);
            var ipEnd = IPAddress.Parse(IpEnd);

            var startBytes = ipStart.GetAddressBytes();
            var endBytes = ipEnd.GetAddressBytes();
            var checkBytes = ipToCheck.GetAddressBytes();

            var graterOrEqual = true;
            var lessOrEqual = true;

            for (var i = 0; i < checkBytes.Length; i++)
            {
                if (checkBytes[i] < startBytes[i])
                {
                    graterOrEqual = false;
                    break;
                }
                if (checkBytes[i] > endBytes[i])
                {
                    lessOrEqual = false;
                    break;
                }
            }

            return graterOrEqual && lessOrEqual;
        }
        catch
        {
            return true;
        }
    }
}