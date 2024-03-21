using Canteen.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace Canteen.Controllers;

public class OrderController : ControllerBase
{
    private readonly CanteenOrderServices _orderServices;
    private ILogger<OrderController> _logger;

    public OrderController(CanteenOrderServices orderServices)
    {
        _orderServices = orderServices;
    }
    
    [HttpGet]
    [Route("getOrders/{userId}")]
    [ProducesResponseType(typeof(OrderOutputDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetOrders(int userId)
    {
        var result = await _orderServices.GetOrderByUserIdAsync(userId);
        if (result.TryPickT0(out var error, out var response))
        {
            _logger.LogError( $"Error status {error.Status} Detail:{error.Detail}");
            return NotFound(error);
        }
        _logger.LogInformation("All orders of user {userId} found correctly", userId);
        return Ok(response);
    }
}