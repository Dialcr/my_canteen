using AvangTur.Application.Extensions;
using Canteen.DataAccess.Enums;
using Canteen.Services.Abstractions;
using Canteen.Services.Dto.CreateProduct;
using Canteen.Services.Dto.Mapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Canteen.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CanteenProductController(IProductServices productServices,
    ILogger<CanteenProductController> logger) : ControllerBase
{

    [HttpGet]
    [Route("get/{productId}")]
    [AllowAnonymous]
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
    [Route("list")]
    [AllowAnonymous]
    public ActionResult<IEnumerable<ProductOutputDto>> GetAllProducts(int? establishmentId)
    {
        var result = productServices.GetAllProducts(establishmentId);

        logger.LogInformation("CantneeProduct found correctly");

        return Ok(result.Select(x => x.ToProductOutputDto()));
    }

    [HttpPost]
    [Route("create")]
    [Authorize(Roles = nameof(RoleNames.ADMIN))]
    public IActionResult CreateCanteenProduct(CreateProductDto product)
    {
        var result = productServices.CreateCanteenProduct(product);

        if (result.TryPickT0(out var error, out var response))
        {
            logger.LogError($"Error status {error.Status} Detail:{error.Detail}");

            return BadRequest(error);
        }

        logger.LogInformation("CantneeProduct found correctly");

        return Ok(response.ToProductOutputDto());
    }

    [HttpPut]
    [Route("update/{productId}")]
    [Authorize(Roles = nameof(RoleNames.ADMIN))]
    public IActionResult UpdateCanteenProduct(int productId, CreateProductDto product)
    {
        var result = productServices.UpdateCanteenProduct(productId, product);

        if (result.TryPickT0(out var error, out var response))
        {
            logger.LogError($"Error status {error.Status} Detail:{error.Detail}");
            return BadRequest(error);
        }

        logger.LogInformation("CanteenProduct updated correctly");
        return Ok(response.ToProductOutputDto());
    }

    [HttpGet]
    [Route("get/category")]
    [AllowAnonymous]
    public IActionResult GetCantneeProductsByCategory(string categoryProduct, int page, int perPage)
    {
        var result = productServices.GetCantneeProductsByCategoryAsync(categoryProduct);

        if (result.Result.TryPickT0(out var error, out var response))
        {
            logger.LogError($"Error status {error.Status} Detail:{error.Detail}");

            return BadRequest(error);
        }

        logger.LogInformation($"All CantneeProduct of category  found correctly {categoryProduct}");

        return Ok(response.ToPagedResult(page, perPage));
    }

    [HttpGet]
    [Route("get/dietary/restriction")]
    [AllowAnonymous]
    public IActionResult GetCantneeProductsByDietaryRestrictions(string dietaryRestriction, int page, int perPage)
    {
        var result = productServices.GetCantneeProductsByDietaryRestrictions(dietaryRestriction);


        if (result.TryPickT0(out var error, out var response))
        {
            logger.LogError($"Error status {error.Status} Detail:{error.Detail}");

            return BadRequest(error);
        }
        logger.LogInformation($"All CantneeProduct of dietaryRestriction {dietaryRestriction}  found correctly");

        return Ok(response.ToPagedResult(page, perPage));
    }

}