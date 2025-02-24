using Canteen.DataAccess.Enums;

namespace Canteen.Services.Dto.DeliveryTime;

public class CreateDeliveryTimeDto
{
    public int EstablishmentId { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string DeliveryTimeType { get; set; } = string.Empty;
}
