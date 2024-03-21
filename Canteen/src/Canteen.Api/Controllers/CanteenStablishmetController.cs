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
    [ProducesResponseType(typeof(IEnumerable<EstablishmentOutputDto>), StatusCodes.Status200OK)]
    public IActionResult GetAllSEstablishments()
    {
        var establishments =  _establishmentService.GetAllEstablishments();
        return Ok(establishments);
    }

    [HttpGet]
    [Route("getEstablishmentById")]
    [ProducesResponseType(typeof(EstablishmentOutputDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetEstablishmentById(int id)
    {
        var stablish = await _establishmentService.GetEstablishmentByIdAsync(id);

        if (stablish.TryPickT0(out var error, out var establishment))
        {
            _logger.LogError("Establishment not found {error}", error.Detail);
            return NotFound();
        }

        _logger.LogInformation("Establishment found correctly");

        return Ok(establishment);
    }
}