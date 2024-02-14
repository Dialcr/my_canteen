namespace Canteen.Services.Dto;

public class DayMenuDto
{
    public int Id { get; set; }
    public IEnumerable<ProductDayDto> ProductsDay { get; set; } = Enumerable.Empty<ProductDayDto>();
    public DateTime Date { get; set; }
    public int EstablishmentId { get; set; }
}