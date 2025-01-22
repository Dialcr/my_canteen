using System;

namespace Canteen.Services.Dto.DeliveryTime;

public class DeliveryTimeOutputDto
{

    public int Id { get; set; }
    public int EstablishmentId { get; set; }
    public TimeSpan StartTime { get; set; } // sample: 7:00 AM
    public TimeSpan EndTime { get; set; } // sample: 9:00 AM
    public string DeliveryTimeType { get; set; } = string.Empty;

}

public static class DeliveryTimeExtention
{
    public static DeliveryTimeOutputDto ToDeliveryTimeOutputDto(this Canteen.DataAccess.Entities.DeliveryTime deliveryTime)
    {
        return new DeliveryTimeOutputDto()
        {
            Id = deliveryTime.Id,
            DeliveryTimeType = deliveryTime.DeliveryTimeType.ToString(),
            EndTime = deliveryTime.EndTime,
            StartTime = deliveryTime.StartTime,
            EstablishmentId = deliveryTime.EstablishmentId,
        };

    }
}