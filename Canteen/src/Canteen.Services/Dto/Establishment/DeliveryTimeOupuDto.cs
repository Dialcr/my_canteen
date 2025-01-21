using System;

namespace Canteen.Services.Dto.Establishment;

public class DeliveryTimeOupuDto
{
    public TimeSpan StartTime { get; set; } // sample: 7:00 AM
    public TimeSpan EndTime { get; set; } // sample: 9:00 AM
    public string DeliveryTimeType { get; set; } = string.Empty;
    public int Id { get; set; }



}
