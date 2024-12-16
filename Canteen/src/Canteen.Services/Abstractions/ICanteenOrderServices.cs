using System;
using Canteen.Services.Dto;
using Canteen.Services.Dto.CanteenRequest;
using Canteen.Services.Dto.Order;

namespace Canteen.Services.Abstractions;

public interface ICanteenOrderServices
{
    Task<OneOf<ResponseErrorDto, Order>> ApplyDiscountToOrderAsync(int orderId);
    Task<OneOf<ResponseErrorDto, Order>> UpdateTotalsAsync(int orderId);
    Task<OneOf<ResponseErrorDto, Order>> CloseOrderIfAllRequestsClosedAsync(int orderId);
    Task<OneOf<ResponseErrorDto, Order>> CancelOrderAsync(int orderId);
    Task<Order> CreateOrderAsync(CanteenCart cart);
    Task<OneOf<ResponseErrorDto, CanteenRequest>> EditRequestIntoOrderAsync(EditRequestDto requestDto);
    Task<OneOf<ResponseErrorDto, CanteenRequest>> PlanningRequestIntoOrderAsync(int requestId, int establishmentId, DateTime newDateTime);
    Task<OneOf<ResponseErrorDto, RequestOutputDto>> CancelRequestIntoOrderAsync(int requestId);
    Task<OneOf<ResponseErrorDto, OrderOutputDto>> GetOrderByUserIdAsync(int userId);

}
