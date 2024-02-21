namespace Canteen.Services.Dto;

public class RequestDto
{
    public int RequestId { get; set; }
    public List<int>? ProductIds { get; set; }
    public DateTime DateTime { get; set; }
}