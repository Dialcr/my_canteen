using Canteen.DataAccess.Entities;
using Canteen.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace Canteen.Controllers;
[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly CartServices _cartServices;

    public CartController(CartServices cartServices)
    {
        _cartServices = cartServices;
    }
    [HttpGet]
    [Route("getCart/{userId}")]
    [ProducesResponseType(typeof(CartOutputDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]

    public async Task<IActionResult> GetCart(int userId)
    {
        var result = await _cartServices.GetCartByUserIdAsync(userId);
        if (result.TryPickT0(out var error, out var response))
        {
            return NotFound(error);
        }
        return Ok(response);
    }
    [HttpPatch]
    [Route("checkout")]
    [ProducesResponseType(typeof(OrderOutputDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<RequestInputDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApplyDiscountToCart(int cardId)
    {
        var result = await _cartServices.CheckoutAsync(cardId);
        if (result.TryPickT0(out var list, out var response))
        {
            return BadRequest(list);
        }
        return Ok(response);
    } 
    
    [HttpPatch]
    [Route("DeleteRequestIntoCart")]
    [ProducesResponseType(typeof(RequestOutputDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteRequestIntoCart(int userId, int cartId, int requestId)
    {
        var result = await _cartServices.DeleteRequestIntoCartAsync(userId, cartId, requestId);
        if (result.TryPickT0(out var error, out var response))
        {
            return BadRequest(error);
        }
        return Ok(response.ToCanteenRequestOutputDto());
    }
    
}