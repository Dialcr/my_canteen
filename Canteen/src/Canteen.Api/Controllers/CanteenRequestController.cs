using Canteen.DataAccess.Entities;
using Canteen.Services.Dto.CanteenRequest;
using Canteen.Services.Services;
using Microsoft.AspNetCore.Mvc;

namespace Canteen.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CanteenRequestController(ILogger<CanteenRequestController> logger,
    RequestServices requestServices,
    CanteenOrderServices orderServices, 
    CartServices cartServices) : ControllerBase
{
  

    private ActionResult<OneOf<ResponseErrorDto, ICollection<CanteenRequest>>> HandleError(ResponseErrorDto error)
    {
        logger.LogError($"Error status {error.Status} Detail:{error.Detail}");
        return BadRequest(error);
    }

    private void LogRequestFound(int userId)
    {
        logger.LogInformation($"All Request of user {userId} found correctly");
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<RequestOutputDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
    [Route("getRequestList")]
    public async Task<ActionResult<OneOf<ResponseErrorDto, ICollection<CanteenRequest>>>> GetRequestList(int userId)
    {
        var result = await requestServices.RequestsListAsync(userId);

        if (result.TryPickT0(out var error, out var response))
        {
            return HandleError(error);
        }

        LogRequestFound(userId);
        var requestList = response.Select(x => x.ToCanteenRequestWithProductsDto());
        return Ok(requestList);
    }


    [HttpGet]
    [ProducesResponseType(typeof(ICollection<CanteenRequest>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
    [Route("getHistoryRequests")]
    public IActionResult GetHistoryRequests(int userId)
    {
        var result = requestServices.HistoryRequestAsync(userId);

        if (result.Result.TryPickT0(out var error, out var response))
        {

            logger.LogError($"Error status {error.Status} Detail:{error.Detail}");
            return BadRequest(error);
        }


        logger.LogInformation($"History Request of user {userId} found correctly");

        return Ok(response.Select(x => x.ToCanteenRequestWithProductsDto()));
    }

    [HttpPatch]
    [ProducesResponseType(typeof(CanteenRequest), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status400BadRequest)]
    [Route("MoveRequestIntoOrder")]
    public async Task<ActionResult<OneOf<ResponseErrorDto, CanteenRequest>>> MoveRequestIntoOrder(int requestId, DateTime moveDate)
    {
        var result = await requestServices.MoveRequestIntoOrderAsync(requestId, moveDate);

        if (result.TryPickT0(out var error, out var response))
        {
            logger.LogError($"Error status {error.Status} Detail:{error.Detail}");
            return BadRequest(error);
        }
        else if (response.TryPickT0(out var products, out var request))
        {
            logger.LogError($"Products not available");

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
        
        var result = await requestServices.CreateRequestAsync(createRequestDto, userId);

        if (result.TryPickT0(out var errorDto, out var cart))
        {
            logger.LogError(errorDto.Detail);

            return NotFound(errorDto);
        }
        await cartServices.UpdateTotalsIntoCartAsync(cart.Id);
        logger.LogInformation($"Request created successfully ");
        return Ok(cart);
    }

    [HttpPatch]
    [Route("EditRequestIntoOrder")]
    [ProducesResponseType(typeof(CanteenRequest), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EditRequestIntoOrder([FromBody] EditRequestDto requestDto)
    {

        var result = await orderServices
            .EditRequestIntoOrderAsync(requestDto);

        if (result.TryPickT0(out var error, out var request))
        {
            logger.LogError("Error during Editing proccess");
            return BadRequest(error);
        }
        logger.LogInformation("Successfuly Edited");
        return Ok(request.ToCanteenRequestWithProductsDto());
        
    }

    [HttpPost("{requestId}/PlanningRequestIntoOrder")]
    [ProducesResponseType(typeof(RequestOutputDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PlanningRequestIntoOrder(
        int requestId,
        [FromBody] PlanningRequestDto planningRequestDto)
    {
        
        var result = await orderServices.PlanningRequestIntoOrderAsync(requestId, planningRequestDto.EstablishmentId, 
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
        
        var result = await cartServices.PlanningRequestIntoCartAsync(requestId, 
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
        var result = requestServices.GetRequerstInfoById(requestId);

        if (result.TryPickT0(out var error, out var response))
        {
            logger.LogError($"Error status {error.Status} Detail:{error.Detail}");

            return BadRequest(error);
        }

        logger.LogInformation("Requests found correctly");

        return Ok(response);
    }

    [HttpPatch]
    [ProducesResponseType(typeof(RequestOutputDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
    [Route("cancelRequest")]
    public async Task<IActionResult> CancelRequestIntoOrder(int requestId)
    {
        var result = await orderServices.CancelRequestIntoOrderAsync(requestId);

        if (result.TryPickT0(out var error, out var response))
        {
            logger.LogError($"Error status {error.Status} Detail:{error.Detail}");
            return BadRequest(error);
        }

        logger.LogInformation($"Request with id {requestId} canceled correctly");

        return Ok(response);
    }

    [HttpPatch]
    [Route("EditRequestIntoCart")]
    [ProducesResponseType(typeof(RequestOutputDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EditRequestIntoCart(EditRequestDto requestDto)
    {
        var result = await cartServices.EditRequestIntoCartAsync( requestDto);

        if (result.TryPickT0(out var error, out var response))
        {
            logger.LogError($"Error status {error.Status} Detail:{error.Detail}");
            return BadRequest(error);
        }

        logger.LogInformation($"Request with id {requestDto.RequestId} canceled correctly");

        return Ok(response);
    }
    
}