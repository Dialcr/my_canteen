using Canteen.Services.Dto;
using Canteen.Services.Dto.DeliveryTime;
using Canteen.Services.Dto.Responses;

namespace Canteen.Services.Abstractions;

public interface IDeliveryTimeService
{
    OneOf<ResponseErrorDto, DeliveryTimeOutputDto> GetDeliveryTimeById(int id);
    OneOf<ResponseErrorDto, Response<NoContentData>> CreateDeliveryTime(CreateDeliveryTimeDto deliveryTimeDto);
    OneOf<ResponseErrorDto, Response<NoContentData>> UpdateDeliveryTime(UpdateDeliveryTimeDto deliveryTimeDto);
    OneOf<ResponseErrorDto, Response<NoContentData>> DeleteDeliveryTime(int id);
    public OneOf<ResponseErrorDto, IEnumerable<DeliveryTimeOutputDto>> GetAllDeliveryTimes();

}
