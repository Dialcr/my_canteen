using Canteen.DataAccess.Settings;
using Canteen.Services.Abstractions;
using Canteen.Services.Dto.Order;
using Canteen.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace Canteen.Controllers;
[ApiController]
[Route("api/[controller]")]
public class OrderController(ICanteenOrderServices orderServices, ILogger<OrderController> logger, TokenUtil tokenUtil) : ControllerBase
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
}