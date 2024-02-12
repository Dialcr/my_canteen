using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace CompraTodayApi.Middlewares;

public class IpAddressMiddleware : IMiddleware
{
    readonly IpAddressServices _service;

    public IpAddressMiddleware(IpAddressServices ipAddressService)
    {
        _service = ipAddressService;
    }

    public async Task InvokeAsync(
        HttpContext context,
        RequestDelegate next)
    {
        var sidClaim = context.User.FindFirst(ClaimTypes.Sid);

        using var __ = LogContext.PushProperty("UserId", sidClaim?.Value ?? "unknown");

        var ipAddress = _service.GetClientIp(context);

        using var _ = LogContext.PushProperty("UserIpAddress", ipAddress);

        await next(context);
    }
}
