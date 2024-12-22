using Canteen.DataAccess.Settings;
using Canteen.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Canteen.Controllers;
[ApiController]
[Route("api/[controller]")]
public class CartController(ICartServices cartServices, TokenUtil tokenUtil) : ControllerBase
{
    [HttpGet]
    [Route("getCart")]
    // [ProducesResponseType(typeof(CartOutputDto), StatusCodes.Status200OK)]
    // [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]

    public async Task<IActionResult> GetCart()
    {
        string? accessToken = HttpContext
            .Request.Headers["Authorization"]
            .FirstOrDefault()
            ?.Split(" ")
            .Last();
        accessToken = accessToken!.Replace("Bearer", "");
        var userId = tokenUtil.GetUserIdFromToken(accessToken);

        var result = await cartServices.GetCartByUserIdAsync(userId);
        if (result.TryPickT0(out var error, out var response))
        {
            return NotFound(error);
        }
        return Ok(response);
    }
    [HttpPatch]
    [Route("checkout")]
    // [ProducesResponseType(typeof(OrderOutputDto), StatusCodes.Status200OK)]
    // [ProducesResponseType(typeof(IEnumerable<RequestInputDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApplyDiscountToCart(int cardId)
    {
        var result = await cartServices.CheckoutAsync(cardId);
        if (result.TryPickT0(out var list, out var response))
        {
            return BadRequest(list);
        }
        return Ok(response);
    }

    [HttpPatch]
    [Route("delete/cart")]
    // [ProducesResponseType(typeof(RequestOutputDto), StatusCodes.Status200OK)]
    // [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteRequestIntoCart(int cartId, int requestId)
    {
        string? accessToken = HttpContext
            .Request.Headers["Authorization"]
            .FirstOrDefault()
            ?.Split(" ")
            .Last();
        accessToken = accessToken!.Replace("Bearer", "");
        var userId = tokenUtil.GetUserIdFromToken(accessToken);

        var result = await cartServices.DeleteRequestIntoCartAsync(userId, cartId, requestId);
        if (result.TryPickT0(out var error, out var response))
        {
            return BadRequest(error);
        }
        return Ok(response);
    }

}