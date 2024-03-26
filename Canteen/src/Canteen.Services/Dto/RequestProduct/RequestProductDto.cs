namespace Canteen.Services.Dto;

public class RequestProductDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 0;
    public decimal UnitPrice { get; set; }
    public string ProductName { get; set; }="";
    
}