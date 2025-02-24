using Canteen.Services.Dto.DeliveryTime;

namespace Canteen.Services.Dto.Mapper;

public static class DeliveryTimeMapper
{
    public static DeliveryTimeOutputDto ToDeliveryTimeOutputDto(this Canteen.DataAccess.Entities.DeliveryTime deliveryTime)
    {
        return new DeliveryTimeOutputDto
        {
            Id = deliveryTime.Id,
            EstablishmentId = deliveryTime.EstablishmentId,
            StartTime = deliveryTime.StartTime,
            EndTime = deliveryTime.EndTime,
            DeliveryTimeType = deliveryTime.DeliveryTimeType.ToString()
        };
    }
}
