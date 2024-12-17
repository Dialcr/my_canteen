using Canteen.Services.Abstractions;
using Canteen.Services.Dto.Order;
using Canteen.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace Canteen.Controllers;

public class OrderController(ICanteenOrderServices orderServices, ILogger<OrderController> logger) : ControllerBase
{
    [HttpGet]
    [Route("getOrders/{userId}")]
    [ProducesResponseType(typeof(OrderOutputDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOrders(int userId)
    {
        var result = await orderServices.GetOrderByUserIdAsync(userId);
        if (result.TryPickT0(out var error, out var response))
        {
            logger.LogError($"Error status {error.Status} Detail:{error.Detail}");
            return NotFound(error);
        }
        logger.LogInformation("All orders of user {userId} found correctly", userId);
        return Ok(response);
    }
}