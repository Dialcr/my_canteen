using Canteen.DataAccess.Enums;
using Canteen.DataAccess.Settings;
using Canteen.Services.Abstractions;
using Canteen.Services.Dto.EstablishmentCategory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Canteen.Controllers;

[ApiController]
[Route("api/[controller]")]

public class EstablishmentCategoryController(
    IEstablishmentCategoryServices establishmentCategoryService,
    ILogger<EstablishmentCategoryController> logger,
    TokenUtil tokenUtil) : ControllerBase
{

    [Route("get/all")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PagedResponse<EstablishmentCategoryOutputDto>), StatusCodes.Status200OK)]
    public IActionResult GetAllSEstablishmentsAsync(int page, int perPage)
    {
        var establishments = establishmentCategoryService.GetAllEstablishmentsCategory(page, perPage);
        return Ok(establishments);
    }
    [HttpGet]
    [Route("get/all/admin")]
    [Authorize(Roles = nameof(RoleNames.Admin))]
    [ProducesResponseType(typeof(PagedResponse<EstablishmentCategoryOutputDto>), StatusCodes.Status200OK)]
    public IActionResult GeEstablishmentsCategoryAsync(int page, int perPage)
    {
        var establishments = establishmentCategoryService.GetAllEstablishmentsCategory(page, perPage, true);
        return Ok(establishments);
    }

    [HttpPost]
    [Route("create")]
    [Authorize(Roles = nameof(RoleNames.Admin))]
    public async Task<IActionResult> CreateEstablishmentCategory(CreateEstablishmentCategoryDto establishmentCategory)
    {
        var result = await establishmentCategoryService.CreateEstablishmentCategoryAsync(establishmentCategory);

        if (result.TryPickT0(out var error, out var response))
        {
            logger.LogError(error.Detail);
            return NotFound(error);
        }

        logger.LogInformation("Establishment category create correctly");

        return Ok(response);
    }
    [HttpPatch]
    [Authorize(Roles = nameof(RoleNames.Admin))]
    public async Task<IActionResult> DisableEstablishmentCategory(int id)
    {
        var result = await establishmentCategoryService.ChangeStatusEstablishmentCategoryAsync(id);

        if (result.TryPickT0(out var error, out var response))
        {
            logger.LogError(error.Detail);
            return BadRequest(error);
        }

        logger.LogInformation("Establishment category change status correctly");

        return Ok(response);
    }
    [HttpPut]
    [Authorize(Roles = nameof(RoleNames.Admin))]
    public async Task<IActionResult> UpdateEstablishmentCategory(UpdateEstablishmentCategoryDto establishmentCategoryDto)
    {
        var result = await establishmentCategoryService.UpdateEstablishmentCategoryAsync(establishmentCategoryDto);

        if (result.TryPickT0(out var error, out var response))
        {
            logger.LogError(error.Detail);
            return BadRequest(error);
        }
        logger.LogInformation("Establishment category update correctly");

        return Ok(response);
    }
}
