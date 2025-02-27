using System;
using Canteen.Services.Dto;
using Canteen.Services.Dto.CanteenRequest;
using Canteen.Services.Dto.Order;
using Canteen.Services.Dto.Responses;

namespace Canteen.Services.Abstractions;

public interface ICanteenOrderServices
{
    Task<OneOf<ResponseErrorDto, Order>> ApplyDiscountToOrderAsync(int orderId);
    Task<OneOf<ResponseErrorDto, Order>> UpdateTotalsAsync(int orderId);
    Task<OneOf<ResponseErrorDto, Order>> CloseOrderIfAllRequestsClosedAsync(int orderId);
    Task<OneOf<ResponseErrorDto, Order>> CancelOrderAsync(int orderId);
    Task<OneOf<ResponseErrorDto, OrderOutputDto>> CreateOrderAsync(int cartId);
    Task<OneOf<ResponseErrorDto, CanteenRequest>> EditRequestIntoOrderAsync(EditRequestDto requestDto);
    Task<OneOf<ResponseErrorDto, CanteenRequest>> PlanningRequestIntoOrderAsync(int requestId, int establishmentId, DateTime newDateTime);
    Task<OneOf<ResponseErrorDto, RequestOutputDto>> CancelRequestIntoOrderAsync(int requestId);
    Task<OneOf<ResponseErrorDto, IEnumerable<OrderOutputDto>>> GetOrderByUserIdAsync(int userId);
    Task<OneOf<ResponseErrorDto, IEnumerable<OrderOutputDto>>> GetAllOrdersAsync();
    Task<OneOf<ResponseErrorDto, Response<NoContentData>>> CheckoutOrder(int orderId, OrderOwnerDto orderOwner);

}
