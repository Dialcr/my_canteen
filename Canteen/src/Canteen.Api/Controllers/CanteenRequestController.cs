using Canteen.DataAccess.Entities;
using Canteen.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace Canteen.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CanteenRequestController : ControllerBase
{
    readonly RequestServices _requestServices;
    readonly MenuServices _menuServices;
    readonly ILogger<CanteenRequestController> _logger;
    readonly CanteenOrderServices _orderServices;
    private readonly CartServices _cartServices;
    public CanteenRequestController(
        ILogger<CanteenRequestController> logger,
        RequestServices requestServices,
        MenuServices menuServices,
        CanteenOrderServices orderServices, CartServices cartServices)
    {
        _requestServices = requestServices;
        _menuServices = menuServices;
        _logger = logger;
        _orderServices = orderServices;
        _cartServices = cartServices;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<Request>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status404NotFound)]
    [Route("getRequestList")]
    public ActionResult<OneOf<ResponseErrorDto, List<Request>>> GetRequestList(int userId)
    {
        var result = _requestServices.RequestsList(userId);

        if (result.Result.TryPickT0(out var error, out var response))
        {
            _logger.LogError($"Error status {error.Status} Detail:{error.Detail}");
            return BadRequest(error);
        }

        _logger.LogInformation($"All Request of user {userId} found correctly");

        return Ok(response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<Request>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status404NotFound)]
    [Route("getHistoryRequests")]
    public IActionResult GetHistoryRequests(int userId)
    {
        var result = _requestServices.HistoryRequest(userId);

        if (result.Result.TryPickT0(out var error, out var response))
        {

            _logger.LogError($"Error status {error.Status} Detail:{error.Detail}");
            return BadRequest(error);
        }


        _logger.LogInformation($"History Request of user {userId} found correctly");

        return Ok(response);
    }

    [HttpPatch]
    [ProducesResponseType(typeof(Request), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(List<Product>), StatusCodes.Status400BadRequest)]
    [Route("moveRequest")]
    public async Task<IActionResult> MoveRequest(int requestId, DateTime moveDate)
    {
        var result = await _requestServices.MoveRequestIntoOrder(requestId, moveDate);

        if (result.TryPickT0(out var error, out var response))
        {
            _logger.LogError($"Error status {error.Status} Detail:{error.Detail}");
            return BadRequest(error);
        }
        else if (response.TryPickT0(out var products, out var request))
        {
            _logger.LogError($"Products not available");

            return BadRequest(error);
        }

        return Ok(response);
    }

    [HttpPost]
    [Route("makeARequest")]
    [ProducesResponseType(typeof(Request), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> MakeARequest([FromForm] RequestDto requestDto)
    {
        var result = await _requestServices.AddProductsToRequest(requestDto.RequestId, requestDto.ProductIds, requestDto.DateTime);

        if (result.TryPickT0(out var errorDto, out var request))
        {
            _logger.LogError("Product can't be added{error}", errorDto.Detail);

            return NotFound();
        }

        return Ok(request);
    }
    [HttpPost]
    [Route("createRequest")]
    [ProducesResponseType(typeof(Request), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CreateRequest([FromForm] CreateRequestInputDto createRequestDto, int userId)
    {
        var result = await _requestServices.CreateRequest(createRequestDto, userId);

        if (result.TryPickT0(out var errorDto, out var request))
        {
            _logger.LogError(errorDto.Detail);

            return NotFound(errorDto);
        }

        return Ok(request);
    }

    [HttpPut]
    [Route("editRequest")]
    [ProducesResponseType(typeof(Request), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EditRequest([FromBody] EditRequestDto requestDto)
    {

        var result = await _orderServices
            .EditRequestIntoOrder(requestDto);

        return result.Match<IActionResult>(
            request =>
            {
                _logger.LogInformation("Successfuly Edited");
                return Ok(request);
            },
            error =>
            {
                _logger.LogError("Error during Editing proccess");
                return BadRequest(error);
            }
        );
    }

    [HttpPost("{requestId}/plan")]
    public async Task<IActionResult> PlanningRequest(
        int requestId,
        
        [FromBody] PlanningRequestDto planningRequestDto)
    {
        
        var result = await _orderServices.PlanningRequestIntoOrder(requestId, planningRequestDto.EstablishmentId, 
            planningRequestDto.NewDateTime);

        return result.Match<IActionResult>(
            request =>
            {
                _logger.LogInformation("Planning Successful");
                return Ok(request);
            },
            error =>
            {
                _logger.LogError("Can't be planned");
                return BadRequest(error);
            }
        );
    }
    [HttpPost("{requestId}/plan/cart")]
    [ProducesResponseType(typeof(Request), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PlanningRequestCart(
        int requestId,
        int cartId,
        
        [FromBody] PlanningRequestDto planningRequestDto)
    {
        
        var result = await _cartServices.PlanningRequestIntoCart(requestId, 
            planningRequestDto.NewDateTime,cartId);

        return result.Match<IActionResult>(
            request =>
            {
                _logger.LogInformation("Planning Successful");
                return Ok(request);
            },
            error =>
            {
                _logger.LogError("Can't be planned");
                return BadRequest(error);
            }
        );
    }

    [HttpGet]
    [ProducesResponseType(typeof(Request), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status404NotFound)]
    [Route("getRequest")]
    public IActionResult GetRequest(int requestId)
    {
        var result = _requestServices.GetRequerstInfoById(requestId);

        if (result.TryPickT0(out var error, out var response))
        {
            _logger.LogError($"Error status {error.Status} Detail:{error.Detail}");

            return BadRequest(error);
        }

        _logger.LogInformation("Requests found correctly");

        return BadRequest(error);

        return Ok(response);
    }

    [HttpPatch]
    [HttpGet]
    [ProducesResponseType(typeof(Request), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status404NotFound)]
    [Route("cancelRequest")]
    public async Task<IActionResult> CancelRequest(int requestId)
    {
        var result = await _orderServices.CancelRequestIntoOrder(requestId);

        if (result.TryPickT0(out var error, out var response))
        {
            _logger.LogError($"Error status {error.Status} Detail:{error.Detail}");
            return BadRequest(error);
        }

        _logger.LogInformation($"Request with id {requestId} canceled correctly");

        return Ok(response);
    }

    [HttpPatch]
    [Route("EditRequestIntoCart")]
    [ProducesResponseType(typeof(Request), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EditRequestIntoCart(EditRequestDto requestDto)
    {
        var result = await _cartServices.EditRequestIntoCart( requestDto);

        if (result.TryPickT0(out var error, out var response))
        {
            _logger.LogError($"Error status {error.Status} Detail:{error.Detail}");
            return BadRequest(error);
        }

        _logger.LogInformation($"Request with id {requestDto.RequestId} canceled correctly");

        return Ok(response);
    }
    
}