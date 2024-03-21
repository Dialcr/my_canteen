using System.Text.Json;
using Canteen.DataAccess;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Canteen.Services.IpAdress;

public class IpAddressServices(ILogger<IpAddressServices> logger,
    EntitiesContext entitiesContext)
{
    

    public string GetClientIp(HttpContext httpContext)
    {
        if (httpContext.Request.Headers.TryGetValue("X-Forwarded-For", out var header))
        {
            var clientIpAddress = header.FirstOrDefault();

            if (clientIpAddress is not null)
            {
                logger.LogDebug("User IP addresses: {ipAddresses}", clientIpAddress);

                if (!clientIpAddress.Contains(','))
                    return clientIpAddress;

                var ipAddresses = clientIpAddress.Split(",");

                var realIpList = ipAddresses
                    .FirstOrDefault();

                return realIpList?.Trim() ?? "";
            }
        }

        var ip = httpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();

        return ip ?? "";
    }

    
    
     async Task<IEnumerable<IpAddressRange>> GetIpAddressRanges()
    {
        var blockedIpRanges = await entitiesContext.KeyValueData
            .FirstOrDefaultAsync(x => x.Key == "ip_ranges_blocked");

        if (blockedIpRanges is null)
            return Enumerable.Empty<IpAddressRange>();

        var ipAddressRanges = JsonSerializer.Deserialize<List<IpAddressRange>>(blockedIpRanges.Data) ?? Enumerable.Empty<IpAddressRange>();

        return ipAddressRanges;
    }

    public async Task<bool> IsIpAddressBlocked(string? ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress))
            return true;

        var ipAddressRanges = await GetIpAddressRanges();

        foreach (var range in ipAddressRanges)
        {
            var ipRange = new IpAddressRange()
            {
                IpStart = range.IpStart,
                IpEnd = range.IpEnd
            };

            var result = ipRange.IsIpInRange(ipAddress);
            if (result)
                return true;
        }

        return false;
    }

    
}