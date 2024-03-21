namespace Canteen.Services.Dto;

public class RequestDto
{
    public int RequestId { get; set; }
    public IEnumerable<int>? ProductIds { get; set; }
    public DateTime DateTime { get; set; }
    
    public int DeliveryTimeId { get; set; }
}