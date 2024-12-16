using System;
using Canteen.Services.Dto;
using Canteen.Services.Dto.CanteenRequest;

namespace Canteen.Services.Abstractions;

public interface IRequestServices
{
    Task<OneOf<ResponseErrorDto, CanteenRequest>> AddProductsToRequestAsync(int requestId, List<int> productIds, DateTime dateTime);
    Task<OneOf<ResponseErrorDto, CartOutputDto>> CreateRequestAsync(CreateRequestInputDto createRequestInputDto, int userId);
    Task<OneOf<ResponseErrorDto, ICollection<CanteenRequest>>> RequestsListAsync(int userId);
    Task<OneOf<ResponseErrorDto, List<CanteenRequest>>> HistoryRequestAsync(int userId);
    Task<OneOf<ResponseErrorDto, IEnumerable<ProductOutputDto>, RequestOutputDto>> MoveRequestIntoOrderAsync(int requestId, DateTime newDeliveryDate);
    Task<OneOf<ResponseErrorDto, CanteenRequest>> MoveRequestIntoCartAsync(int requestId, DateTime newDeliveryDate);
    OneOf<ResponseErrorDto, RequestOutputDto> GetRequerstInfoById(int requestId);
    ICollection<RequestProductOutputDto>? AllProductsOk(CanteenRequest canteenRequest, Menu menu);
    Task<OneOf<ResponseErrorDto, CanteenRequest>> DiscountFromInventaryAsync(CanteenRequest canteenRequest, int establishmentId);

}
