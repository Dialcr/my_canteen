using Canteen.DataAccess.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Canteen.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CanteenProductController : ControllerBase
{
    readonly ProductServices _productServices;
    readonly ILogger<CanteenProductController> _logger;

    public CanteenProductController(
        ProductServices productServices,
        ILogger<CanteenProductController> logger)
    {
        _productServices = productServices;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status404NotFound)]
    [Route("getCantneeProductById")]
    public OneOf<ResponseErrorDto, Product> GetCantneeProductById(int productId)
    {
        var result = _productServices.GetCantneeProductById(productId);

        if (result.TryPickT0(out var error, out var response))
        {
            _logger.LogError($"Error status {error.Status} Detail:{error.Detail}");

            return error;
        }

        _logger.LogInformation("CantneeProduct found correctly");

        return response;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status404NotFound)]
    [Route("getCantneeProductsByCategory")]
    public OneOf<ResponseErrorDto, List<Product>> GetCantneeProductsByCategory(string categoryProduct)
    {
        var result = _productServices.GetCantneeProductsByCategory(categoryProduct);

        if (result.TryPickT0(out var error, out var response))
        {
            _logger.LogError($"Error status {error.Status} Detail:{error.Detail}");

            return error;
        }

        _logger.LogInformation($"All CantneeProduct of category  found correctly {categoryProduct}");

        return response;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status404NotFound)]
    [Route("getCantneeProductsByDietaryRestrictions")]
    public OneOf<ResponseErrorDto, List<Product>> GetCantneeProductsByDietaryRestrictions(string dietaryRestriction)
    {
        var result = _productServices.GetCantneeProductsByDietaryRestrictions(dietaryRestriction);

        if (result.TryPickT0(out var error, out var response))
        {
            _logger.LogError($"Error status {error.Status} Detail:{error.Detail}");

            return error;
        }

        _logger.LogInformation($"All CantneeProduct of dietaryRestriction {dietaryRestriction}  found correctly");

        return response;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<DayProduct>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status404NotFound)]
    [Route("getCantneeProductsByMenu")]
    public async Task<OneOf<ResponseErrorDto, List<DayProduct>>> GetCantneeProductsByMenu(Menu dayMenu)
    {
        var result = await _productServices.GetCantneeProductsByMenu(dayMenu);

        if (result.TryPickT0(out var error, out var response))
        {
            return error;
        }

        _logger.LogInformation($"All CantneeProduct of menu {dayMenu.Id} form establishment {dayMenu.IdEstablishment} found correctly");

        return response;
    }
}
