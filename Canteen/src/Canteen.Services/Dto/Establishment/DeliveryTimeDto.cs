using System;
using Canteen.DataAccess.Enums;

namespace Canteen.Services.Dto.Establishment;

public class DeliveryTimeDto
{

    public TimeSpan StartTime { get; set; } // sample: 7:00 AM
    public TimeSpan EndTime { get; set; } // sample: 9:00 AM
    public string DeliveryTimeType { get; set; } = string.Empty;
}
