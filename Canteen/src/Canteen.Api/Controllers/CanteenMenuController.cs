using Canteen.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace Canteen.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CanteenMenuController : ControllerBase
{
    readonly MenuServices _menuServices;
    readonly ILogger<CanteenMenuController> _logger;

    public CanteenMenuController(
        MenuServices menuServices,
        ILogger<CanteenMenuController> logger)
    {
        _menuServices = menuServices;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(MenuOutputDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
    [Route("getMenuByEstablishmentAndDate")]
    public IActionResult GetMenuByEstablishmentDate(
        int idEstablishment,
        DateTime date)
    {
        var result = _menuServices.GetMenuByEstablishmentAndDate(idEstablishment, date);

        if (result.TryPickT0(out var error, out var response))
        {
            _logger.LogError($"Error status {error.Status} Detail:{error.Detail}");

            return BadRequest(error);
        }

        _logger.LogInformation($"Menu from establishment {idEstablishment} in date {date}found correctly");

        return Ok(response.ToEstablishmentOutputDto());
    }
}
