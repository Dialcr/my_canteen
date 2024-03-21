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
    [ProducesResponseType(typeof(List<RequestOutputDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
    [Route("getRequestList")]
    public async Task<ActionResult<OneOf<ResponseErrorDto, ICollection<CanteenRequest>>>> GetRequestList(int userId)
    {
        var result = await _requestServices.RequestsListAsync(userId);

        if (result.TryPickT0(out var error, out var response))
        {
            _logger.LogError($"Error status {error.Status} Detail:{error.Detail}");
            return BadRequest(error);
        }

        _logger.LogInformation($"All Request of user {userId} found correctly");
        var requestList = response.Select(x => x.ToCanteenRequestWithProductsDto());
        return Ok(requestList);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ICollection<CanteenRequest>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
    [Route("getHistoryRequests")]
    public IActionResult GetHistoryRequests(int userId)
    {
        var result = _requestServices.HistoryRequestAsync(userId);

        if (result.Result.TryPickT0(out var error, out var response))
        {

            _logger.LogError($"Error status {error.Status} Detail:{error.Detail}");
            return BadRequest(error);
        }


        _logger.LogInformation($"History Request of user {userId} found correctly");

        return Ok(response.Select(x => x.ToCanteenRequestWithProductsDto()));
    }

    [HttpPatch]
    [ProducesResponseType(typeof(CanteenRequest), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status400BadRequest)]
    [Route("MoveRequestIntoOrder")]
    public async Task<ActionResult<OneOf<ResponseErrorDto, CanteenRequest>>> MoveRequestIntoOrder(int requestId, DateTime moveDate)
    {
        var result = await _requestServices.MoveRequestIntoOrderAsync(requestId, moveDate);

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
    [Route("createRequest")]
    [ProducesResponseType(typeof(CartOutputDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRequest([FromBody] CreateRequestInputDto createRequestDto, int userId)
    {
        
        var result = await _requestServices.CreateRequestAsync(createRequestDto, userId);

        if (result.TryPickT0(out var errorDto, out var cart))
        {
            _logger.LogError(errorDto.Detail);

            return NotFound(errorDto);
        }
        await _cartServices.UpdateTotalsIntoCartAsync(cart.Id);
        _logger.LogInformation($"Request created successfully ");
        return Ok(cart);
    }

    [HttpPatch]
    [Route("EditRequestIntoOrder")]
    [ProducesResponseType(typeof(CanteenRequest), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EditRequestIntoOrder([FromBody] EditRequestDto requestDto)
    {

        var result = await _orderServices
            .EditRequestIntoOrderAsync(requestDto);

        if (result.TryPickT0(out var error, out var request))
        {
            _logger.LogError("Error during Editing proccess");
            return BadRequest(error);
        }
        _logger.LogInformation("Successfuly Edited");
        return Ok(request.ToCanteenRequestWithProductsDto());
        
    }

    [HttpPost("{requestId}/PlanningRequestIntoOrder")]
    [ProducesResponseType(typeof(RequestOutputDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PlanningRequestIntoOrder(
        int requestId,
        [FromBody] PlanningRequestDto planningRequestDto)
    {
        
        var result = await _orderServices.PlanningRequestIntoOrderAsync(requestId, planningRequestDto.EstablishmentId, 
            planningRequestDto.NewDateTime);

        if (result.TryPickT0(out var error, out var request))
        {
            return BadRequest(error);
        }
        return Ok(request.ToCanteenRequestWithProductsDto());
    }
    [HttpPost("{requestId}/plan/cart")]
    [ProducesResponseType(typeof(RequestOutputDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PlanningRequestCart(
        int requestId,
        [FromBody] PlanningRequestDto planningRequestDto)
    {
        
        var result = await _cartServices.PlanningRequestIntoCartAsync(requestId, 
            planningRequestDto.NewDateTime);

        if (result.TryPickT0(out var error, out var request))
        {
            return BadRequest(error);
        }

        return Ok(request.ToCanteenRequestOutputDto());

    }

    [HttpGet]
    [ProducesResponseType(typeof(RequestOutputDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
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

        return Ok(response);
    }

    [HttpPatch]
    [ProducesResponseType(typeof(RequestOutputDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
    [Route("cancelRequest")]
    public async Task<IActionResult> CancelRequestIntoOrder(int requestId)
    {
        var result = await _orderServices.CancelRequestIntoOrderAsync(requestId);

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
    [ProducesResponseType(typeof(RequestOutputDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EditRequestIntoCart(EditRequestDto requestDto)
    {
        var result = await _cartServices.EditRequestIntoCartAsync( requestDto);

        if (result.TryPickT0(out var error, out var response))
        {
            _logger.LogError($"Error status {error.Status} Detail:{error.Detail}");
            return BadRequest(error);
        }

        _logger.LogInformation($"Request with id {requestDto.RequestId} canceled correctly");

        return Ok(response);
    }
    
}