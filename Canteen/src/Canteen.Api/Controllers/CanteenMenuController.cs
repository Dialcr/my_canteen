using Canteen.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace Canteen.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CanteenMenuController( MenuServices menuServices,
 ILogger<CanteenMenuController> logger) : ControllerBase
{

    [HttpGet]
    [ProducesResponseType(typeof(MenuOutputDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
    [Route("getMenuByEstablishmentAndDate")]
    public IActionResult GetMenuByEstablishmentDate(
        int idEstablishment,
        DateTime date)
    {
        var result = menuServices.GetMenuByEstablishmentAndDate(idEstablishment, date);

        if (result.TryPickT0(out var error, out var response))
        {
            logger.LogError($"Error status {error.Status} Detail:{error.Detail}");

            return BadRequest(error);
        }

        logger.LogInformation($"Menu from establishment {idEstablishment} in date {date}found correctly");

        return Ok(response.ToEstablishmentOutputDto());
    }
}
