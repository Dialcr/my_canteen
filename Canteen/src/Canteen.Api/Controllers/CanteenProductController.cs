using Canteen.DataAccess.Entities;
using Canteen.DataAccess.Enums;
using Canteen.Services.Abstractions;
using Canteen.Services.Dto.Mapper;
using Canteen.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace Canteen.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CanteenProductController(IProductServices productServices,
    ILogger<CanteenProductController> logger) : ControllerBase
{

    [HttpGet]
    [ProducesResponseType(typeof(ProductOutputDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
    [Route("GetCanteenProductById")]
    public IActionResult GetCanteenProductById(int productId)
    {
        var result = productServices.GetCantneeProductById(productId);

        if (result.TryPickT0(out var error, out var response))
        {
            logger.LogError($"Error status {error.Status} Detail:{error.Detail}");

            return BadRequest(error);
        }

        logger.LogInformation("CantneeProduct found correctly");

        return Ok(response.ToProductOutputDto());
    }

    [HttpGet]
    [ProducesResponseType(typeof(ICollection<ProductOutputDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
    [Route("getCantneeProductsByCategory")]
    public IActionResult GetCantneeProductsByCategory(ProductCategory categoryProduct)
    {
        var result = productServices.GetCantneeProductsByCategoryAsync(categoryProduct);

        if (result.Result.TryPickT0(out var error, out var response))
        {
            logger.LogError($"Error status {error.Status} Detail:{error.Detail}");

            return BadRequest(error);
        }

        logger.LogInformation($"All CantneeProduct of category  found correctly {categoryProduct}");

        return Ok(response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ICollection<ProductOutputDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
    [Route("getCantneeProductsByDietaryRestrictions")]
    public IActionResult GetCantneeProductsByDietaryRestrictions(string dietaryRestriction)
    {
        var result = productServices.GetCantneeProductsByDietaryRestrictions(dietaryRestriction);


        if (result.TryPickT0(out var error, out var response))
        {
            logger.LogError($"Error status {error.Status} Detail:{error.Detail}");

            return BadRequest(error);
        }
        logger.LogInformation($"All CantneeProduct of dietaryRestriction {dietaryRestriction}  found correctly");

        return Ok(result);
    }

}