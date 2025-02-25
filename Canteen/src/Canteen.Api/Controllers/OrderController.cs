using Canteen.DataAccess.Enums;
using Canteen.DataAccess.Settings;
using Canteen.Services.Abstractions;
using Canteen.Services.Dto.Order;
using Canteen.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Canteen.Controllers;
[ApiController]
[Route("api/[controller]")]
public class OrderController(ICanteenOrderServices orderServices, IRequestServices requestServices, ILogger<OrderController> logger, TokenUtil tokenUtil) : ControllerBase
{
    [HttpGet]
    [Route("get")]
    public async Task<IActionResult> GetOrders()
    {
        string? accessToken = HttpContext
            .Request.Headers["Authorization"]
            .FirstOrDefault()
            ?.Split(" ")
            .Last();
        accessToken = accessToken!.Replace("Bearer", "");
        var userId = tokenUtil.GetUserIdFromToken(accessToken);

        var result = await orderServices.GetOrderByUserIdAsync(userId);
        if (result.TryPickT0(out var error, out var response))
        {
            logger.LogError($"Error status {error.Status} Detail:{error.Detail}");
            return NotFound(error);
        }
        logger.LogInformation("All orders of user {userId} found correctly", userId);
        return Ok(response);
    }

    [HttpGet]
    [Route("all")]
    [Authorize(Roles = nameof(RoleNames.ADMIN))]
    public async Task<IActionResult> GetAllOrders()
    {
        var result = await orderServices.GetAllOrdersAsync();
        if (result.TryPickT0(out var error, out var response))
        {
            logger.LogError($"Error status {error.Status} Detail:{error.Detail}");
            return NotFound(error);
        }
        logger.LogInformation("All orders found correctly");
        return Ok(response);
    }

    [HttpPut]
    [Route("request/{requestId}/status")]
    [Authorize(Roles = nameof(RoleNames.ADMIN))]
    public async Task<IActionResult> ChangeRequestStatus(int requestId, RequestStatus newStatus)
    {
        var result = await requestServices.ChangeRequestStatusAsync(requestId, newStatus);
        if (result.TryPickT0(out var error, out var response))
        {
            logger.LogError($"Error status {error.Status} Detail:{error.Detail}");
            return BadRequest(error);
        }

        logger.LogInformation("Request status updated correctly");
        return Ok(response);
    }
}