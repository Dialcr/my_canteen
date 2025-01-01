using Canteen.DataAccess.Enums;
using Canteen.Services.Abstractions;
using Canteen.Services.Dto.Menu;
using Canteen.Services.Dto.Responses;
using Canteen.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Canteen.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CanteenMenuController(IMenuServices menuServices,
 ILogger<CanteenMenuController> _logger) : ControllerBase
{
    // public readonly string Name { get; set; }


    [HttpGet]
    // [ProducesResponseType(typeof(MenuOutputDto), StatusCodes.Status200OK)]
    // [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
    [Route("get")]
    public IActionResult GetMenuByEstablishmentDate(
        int establishmentId,
        DateTime date)
    {
        var result = menuServices.GetMenuByEstablishmentAndDate(establishmentId, date);

        if (result.TryPickT0(out var error, out var response))
        {
            _logger.LogError($"Error status {error.Status} Detail:{error.Detail}");

            return BadRequest(error);
        }

        _logger.LogInformation($"Menu from establishment {establishmentId} in date {date}found correctly");

        return Ok(response.ToMenuOutputDto());
    }
    [HttpPost]
    [ProducesResponseType(typeof(Response<NoContentData>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
    [Route("create")]
    [Authorize(Roles = nameof(RoleNames.Admin))]
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
        return Ok(new Response<NoContentData>());
    }
    [HttpGet]
    [Route("get/test")]
    [AllowAnonymous]
    public IActionResult GetTest()
    {
        return Ok();
    }

}
