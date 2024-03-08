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
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status404NotFound)]

    public async Task<IActionResult> GetCart(int userId)
    {
        var result = await _cartServices.GetCartByUserId(userId);
        if (result.TryPickT0(out var error, out var response))
        {
            return NotFound(error);
        }
        return Ok(response);
    }
    [HttpPatch]
    [Route("checkout")]
    [ProducesResponseType(typeof(OrderOutputDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ApplyDiscountToCart(int cardId)
    {
        var result = await _cartServices.Checkout(cardId);
        if (result.TryPickT0(out var error, out var response))
        {
            return BadRequest(error);
        }
        else if (response.TryPickT0(out var collection, out var order))
        {
            return BadRequest(collection);
        }
        return Ok(response.AsT1);
    } 
    
    [HttpPatch]
    [Route("DeleteRequestIntoCart")]
    [ProducesResponseType(typeof(RequestOutputDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteRequestIntoCart(int userId, int cartId, int requestId)
    {
        var result = await _cartServices.DeleteRequestIntoCart(userId, cartId, requestId);
        if (result.TryPickT0(out var error, out var response))
        {
            return BadRequest(error);
        }
        return Ok(response.ToCanteenRequestOutputDto());
    }
    
}