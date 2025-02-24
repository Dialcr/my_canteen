namespace Canteen.Services.Dto.DeliveryTime;

public class UpdateDeliveryTimeDto
{
    public int Id { get; set; }
    public int EstablishmentId { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string DeliveryTimeType { get; set; } = string.Empty;
}
