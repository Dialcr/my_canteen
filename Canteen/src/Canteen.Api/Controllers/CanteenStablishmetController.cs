using Canteen.DataAccess.Entities;
using Canteen.DataAccess.Enums;
using Canteen.DataAccess.Settings;
using Canteen.Services.Abstractions;
using Canteen.Services.Dto.Establishment;
using Canteen.Services.Dto.Responses;
using Canteen.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Canteen.Controllers;
[ApiController]
[Route("api/[controller]")]
public class CanteenStablishmentController(
    IEstablishmentService establishmentService,
    ILogger<CanteenStablishmentController> logger,
    TokenUtil tokenUtil) : ControllerBase
{

    [HttpGet]
    [Route("get/all")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllSEstablishmentsAsync(int page, int perPage, int? category)
    {
        var establishments = establishmentService.GetAllEstablishments(page, perPage, category);
        return Ok(establishments);
    }
    [HttpGet]
    [Route("get/popular")]
    [AllowAnonymous]
    public IActionResult GetMustPopularEstablishments(int page, int perPage)
    {
        var establishments = establishmentService.GetAllEstablishments(page, perPage);
        return Ok(establishments);
    }
    [HttpGet]
    [Route("get/all/admin")]
    [Authorize(Roles = nameof(RoleNames.ADMIN))]
    public async Task<IActionResult> GeSEstablishmentsAsync(int page, int perPage)
    {
        var establishments = establishmentService.GetAllEstablishments(page, perPage, useInactive: true);
        return Ok(establishments);
    }

    [HttpGet]
    [Route("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEstablishmentById(int id)
    {
        var stablish = await establishmentService.GetEstablishmentByIdAsync(id);

        if (stablish.TryPickT0(out var error, out var establishment))
        {
            logger.LogError("Establishment not found {error}", error.Detail);
            return NotFound();
        }

        logger.LogInformation("Establishment found correctly");

        return Ok(establishment);
    }
    [HttpGet]
    [Route("delivery/times")]
    public async Task<IActionResult> GetDeliveryTimesByEstablishment(int id, int page, int perPage)
    {
        var resutl = establishmentService.GetDeliveryTimesEstablishment(id, page, perPage);

        if (resutl.TryPickT0(out var error, out var deliveryTimes))
        {
            logger.LogError("Establishment not found {error}", error.Detail);
            return NotFound();
        }

        logger.LogInformation("Delivery times correctly founded");

        return Ok(deliveryTimes);
    }
    [HttpPost]
    [Route("create")]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> CreateEstablishment([FromBody] CreateEstablismentDto establishment)
    {
        var result = await establishmentService.CreateEstablishmentAsync(establishment);

        if (result.TryPickT0(out var error, out var response))
        {
            logger.LogError(error.Detail);
            return BadRequest(error);
        }

        logger.LogInformation("Establishment create correctly");

        return Ok(response);
    }
    [HttpPut]
    [Route("update")]
    [Authorize(Roles = nameof(RoleNames.ADMIN))]
    public async Task<IActionResult> UpdateEstablishment(UpdateEstablismentDto establishmen)
    {
        var result = await establishmentService.UpdateEstablishmentAsync(establishmen);

        if (result.TryPickT0(out var error, out var response))
        {
            logger.LogError(error.Detail);
            return BadRequest(error);
        }

        logger.LogInformation("Establishment updated correctly");

        return Ok(response);
    }
    [HttpPatch]
    [Authorize(Roles = "ADMIN")]
    public async Task<IActionResult> DisableEstablishment(int id)
    {
        var result = await establishmentService.ChangeStatusEstablishmentAsync(id);

        if (result.TryPickT0(out var error, out var response))
        {
            logger.LogError(error.Detail);
            return BadRequest(error);
        }

        logger.LogInformation("Establishment change status  correctly");

        return Ok(response);
    }

}