namespace Canteen.Services.Dto;

public class MenuProductInputDto
{
    public Product Product { get; set; }
    public int ProductId { get; set; }
    public MenuProduct MenuProduct { get; set; }
    public int MenuProductId { get; set; }
    public int Quantity { get; set; }
}