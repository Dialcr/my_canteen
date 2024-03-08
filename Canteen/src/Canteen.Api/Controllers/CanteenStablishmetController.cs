using Canteen.DataAccess.Entities;
using Canteen.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace Canteen.Controllers;
[ApiController]
[Route("Canteen/[controller]")]
public class CanteenStablishmentController : ControllerBase
{
    EstablishmentService _establishmentService;
    ILogger<CanteenStablishmentController> _logger;

    public CanteenStablishmentController(
        EstablishmentService establishmentService,
        ILogger<CanteenStablishmentController> logger)
    {
        _establishmentService = establishmentService;
        _logger = logger;
    }

    [HttpGet]
    [Route("getAllsEstablishments")]
    [ProducesResponseType(typeof(ICollection<EstablishmentOutputDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status404NotFound)]
    public IActionResult GetAllSEstablishments()
    {
        var establishments =  _establishmentService.GetAllEstablishmentsAsync();

        if (establishments.TryPickT0(out var error, out var stablList))
        {
            _logger.LogError("There are no establishment to be found {error}", error.Detail);

            return NotFound();
        }

        _logger.LogInformation("All Establishment list");

        return Ok(stablList);
    }

    [HttpGet]
    [Route("getEstablishmentById")]
    [ProducesResponseType(typeof(EstablishmentOutputDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEstablishmentById(int id)
    {
        var stablish = await _establishmentService.GetEstablishmentById(id);

        if (stablish.TryPickT0(out var error, out var establishment))
        {
            _logger.LogError("Establishment not found {error}", error.Detail);
            return NotFound();
        }

        _logger.LogInformation("Establishment found correctly");

        return Ok(establishment);
    }
    [HttpGet]
    [Route("getAllEstablishment")]
    [ProducesResponseType(typeof(ICollection<EstablishmentOutputDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status404NotFound)]
    public IActionResult GetAllEstablishment()
    {
        var stablish =  _establishmentService.GetAllEstablishment();

        if (stablish.TryPickT0(out var error, out var establishment))
        {
            _logger.LogError(error.Detail);
            return NotFound();
        }

        _logger.LogInformation("Establishments found correctly");

        return Ok(establishment);
    }
}