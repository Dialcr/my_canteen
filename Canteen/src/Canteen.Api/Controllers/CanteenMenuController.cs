using Canteen.Services.Abstractions;
using Canteen.Services.Dto.Menu;
using Canteen.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace Canteen.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CanteenMenuController(IMenuServices menuServices,
 ILogger<CanteenMenuController> _logger) : ControllerBase
{

    [HttpGet]
    [ProducesResponseType(typeof(MenuOutputDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
    [Route("get")]
    public IActionResult GetMenuByEstablishmentDate(
        int idEstablishment,
        DateTime date)
    {
        var result = menuServices.GetMenuByEstablishmentAndDate(idEstablishment, date);

        if (result.TryPickT0(out var error, out var response))
        {
            _logger.LogError($"Error status {error.Status} Detail:{error.Detail}");

            return BadRequest(error);
        }

        _logger.LogInformation($"Menu from establishment {idEstablishment} in date {date}found correctly");

        return Ok(response.ToMenuOutputDto());
    }
    [HttpPost]
    [ProducesResponseType(typeof(MenuOutputDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
    [Route("create")]
    public IActionResult CreateMenu(
        CreateMenuInputDto menu)
    {
        _logger.LogInformation("Creating a new menu");

        var result = menuServices.CreateMenu(menu);
        if (result.TryPickT0(out var error, out var response))
        {
            _logger.LogError($"Error status {error.Status} Detail:{error.Detail}");
            return BadRequest(error);
        }
        _logger.LogInformation("Saving menu");
        return Ok();
    }
}
