using Canteen.DataAccess.Entities;
using Canteen.DataAccess.Enums;
using Canteen.Services.Services;
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
    [ProducesResponseType(typeof(ProductOutputDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status404NotFound)]
    [Route("getCantneeProductById")]
    public IActionResult GetCantneeProductById(int productId)   
    {
        var result = _productServices.GetCantneeProductById(productId);

        if (result.TryPickT0(out var error, out var response))
        {
            _logger.LogError($"Error status {error.Status} Detail:{error.Detail}");

            return BadRequest(error);
        }

        _logger.LogInformation("CantneeProduct found correctly");

        return Ok(response.ToProductOutputDto());
    }

    [HttpGet]
    [ProducesResponseType(typeof(ICollection<ProductOutputDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status404NotFound)]
    [Route("getCantneeProductsByCategory")]
    public IActionResult GetCantneeProductsByCategory(ProductCategory categoryProduct)
    {
        var result = _productServices.GetCantneeProductsByCategory(categoryProduct);

        if (result.Result.TryPickT0(out var error, out var response))
        {
            _logger.LogError($"Error status {error.Status} Detail:{error.Detail}");

            return BadRequest(error);
        }

        _logger.LogInformation($"All CantneeProduct of category  found correctly {categoryProduct}");

        return Ok(response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ICollection<ProductOutputDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status404NotFound)]
    [Route("getCantneeProductsByDietaryRestrictions")]
    public IActionResult GetCantneeProductsByDietaryRestrictions(string dietaryRestriction)
    {
        var result = _productServices.GetCantneeProductsByDietaryRestrictions(dietaryRestriction);

        
        if (result.TryPickT0(out var error, out var response))
        {
            _logger.LogError($"Error status {error.Status} Detail:{error.Detail}");

            return BadRequest(error);
        }
        _logger.LogInformation($"All CantneeProduct of dietaryRestriction {dietaryRestriction}  found correctly");

        return Ok(result);
    }
    
}