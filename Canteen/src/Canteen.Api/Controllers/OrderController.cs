using Canteen.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace Canteen.Controllers;

public class OrderController : ControllerBase
{
    private readonly CanteenOrderServices _orderServices;

    public OrderController(CanteenOrderServices orderServices)
    {
        _orderServices = orderServices;
    }
    
    [HttpGet]
    [Route("getOrders/{userId}")]
    [ProducesResponseType(typeof(OrderOutputDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCart(int userId)
    {
        var result = await _orderServices.GetOrderByUserId(userId);
        if (result.TryPickT0(out var error, out var response))
        {
            return NotFound(error);
        }
        return Ok(response);
    }
}