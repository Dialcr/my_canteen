using Canteen.Services.Abstractions;
using Canteen.Services.Dto.DeliveryTime;
using Canteen.Services.Dto.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Canteen.Services.Dto;
using Canteen.DataAccess.Enums;


namespace Canteen.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeliveryTimeController : ControllerBase
{
    private readonly IDeliveryTimeService _deliveryTimeService;
    private readonly ILogger<DeliveryTimeController> _logger;

    public DeliveryTimeController(IDeliveryTimeService deliveryTimeService, ILogger<DeliveryTimeController> logger)
    {
        _deliveryTimeService = deliveryTimeService;
        _logger = logger;
    }

    [HttpGet]
    [Route("{id}")]
    public IActionResult GetDeliveryTime(int id)
    {
        var result = _deliveryTimeService.GetDeliveryTimeById(id);
        if (result.TryPickT0(out var error, out var response))
        {
            _logger.LogError($"Error status {error.Status} Detail:{error.Detail}");
            return BadRequest(error);
        }
        return Ok(response);
    }

    [HttpPost]
    [Authorize(Roles = nameof(RoleNames.ADMIN))]
    public IActionResult CreateDeliveryTime(CreateDeliveryTimeDto deliveryTimeDto)
    {
        var result = _deliveryTimeService.CreateDeliveryTime(deliveryTimeDto);
        if (result.TryPickT0(out var error, out var response))
        {
            _logger.LogError($"Error status {error.Status} Detail:{error.Detail}");
            return BadRequest(error);
        }
        return Ok(response);
    }

    [HttpPut]
    [Authorize(Roles = nameof(RoleNames.ADMIN))]
    public IActionResult UpdateDeliveryTime(UpdateDeliveryTimeDto deliveryTimeDto)
    {
        var result = _deliveryTimeService.UpdateDeliveryTime(deliveryTimeDto);
        if (result.TryPickT0(out var error, out var response))
        {
            _logger.LogError($"Error status {error.Status} Detail:{error.Detail}");
            return BadRequest(error);
        }
        return Ok(response);
    }

    [HttpDelete]
    [Route("{id}")]
    [Authorize(Roles = nameof(RoleNames.ADMIN))]
    public IActionResult DeleteDeliveryTime(int id)
    {
        var result = _deliveryTimeService.DeleteDeliveryTime(id);
        if (result.TryPickT0(out var error, out var response))
        {
            _logger.LogError($"Error status {error.Status} Detail:{error.Detail}");
            return BadRequest(error);
        }
        return Ok(response);
    }

    [HttpGet]
    public IActionResult GetAllDeliveryTimes()
    {
        var result = _deliveryTimeService.GetAllDeliveryTimes();
        return result.Match<IActionResult>(
            error => StatusCode(error.Status, error.Detail),
            deliveryTimes => Ok(deliveryTimes)
        );
    }
}
