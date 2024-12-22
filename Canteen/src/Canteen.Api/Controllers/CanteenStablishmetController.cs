using Canteen.DataAccess.Entities;
using Canteen.DataAccess.Enums;
using Canteen.Services.Abstractions;
using Canteen.Services.Dto.Establishment;
using Canteen.Services.Dto.Responses;
using Canteen.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Canteen.Controllers;
[ApiController]
[Route("Canteen/[controller]")]
public class CanteenStablishmentController(
    IEstablishmentService establishmentService,
    ILogger<CanteenStablishmentController> logger) : ControllerBase
{

    [HttpGet]
    [Route("get/all")]
    [ProducesResponseType(typeof(IEnumerable<EstablishmentOutputDto>), StatusCodes.Status200OK)]
    public IActionResult GetAllSEstablishments()
    {
        var establishments = establishmentService.GetAllEstablishments();
        return Ok(establishments);
    }

    [HttpGet]
    [Route("{id}")]
    [ProducesResponseType(typeof(EstablishmentOutputDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
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
    [ProducesResponseType(typeof(PagedResponse<DeliveryTimeOupuDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
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
    [ProducesResponseType(typeof(Response<NoContentData>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
    [Authorize(Roles = nameof(RoleNames.Admin))]
    public async Task<IActionResult> CreateEstablishment(CreateEstablismentDto establishmen)
    {
        var result = await establishmentService.CreateEstablishmentAsync(establishmen);

        if (result.TryPickT0(out var error, out var response))
        {
            logger.LogError("Establishment not found {error}", error.Detail);
            return NotFound();
        }

        logger.LogInformation("Establishment create correctly");

        return Ok(response);
    }

}