namespace Canteen.Services.Dto;

public record PagedRequest
{
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
}
