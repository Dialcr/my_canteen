using System;

namespace Canteen.Services.Dto.Establishment;

public class CreateEstablismentDto
{

    public string Name { get; set; }

    public string Description { get; set; }
    public ICollection<DeliveryTimeDto> DeliveryTimes { get; set; } = [];
}
