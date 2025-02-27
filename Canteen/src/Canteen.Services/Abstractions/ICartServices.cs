using System;
using Canteen.Services.Dto;
using Canteen.Services.Dto.CanteenRequest;
using Canteen.Services.Dto.Order;

namespace Canteen.Services.Abstractions;

public interface ICartServices
{
    // Task<OneOf<IEnumerable<RequestProductOutputDto>, OrderOutputDto>> CheckoutAsync(int cartId);
    Task<OneOf<ResponseErrorDto, CanteenCart>> ApplyDiscountToCartAsync(int cardId);
    Task<OneOf<ResponseErrorDto, CanteenCart>> UpdateTotalsIntoCartAsync(int cartId);
    Task<OneOf<ResponseErrorDto, CartOutputDto>> GetCartByUserIdAsync(int userId);
    Task<OneOf<ResponseErrorDto, RequestOutputDto>> EditRequestIntoCartAsync(EditRequestDto requestDto);
    Task<OneOf<ResponseErrorDto, CanteenRequest>> AddProductToRequestCartAsync(int userId, RequestInputDto dto);
    Task<OneOf<ResponseErrorDto, RequestOutputDto>> DeleteRequestIntoCartAsync(int userId, int cartId, int requestId);
    Task<OneOf<ResponseErrorDto, CanteenRequest>> PlanningRequestIntoCartAsync(int requestId, DateTime newDateTime);
}