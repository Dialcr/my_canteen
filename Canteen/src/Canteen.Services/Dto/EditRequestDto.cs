namespace Canteen.Services.Dto;

public class EditRequestDto
{
    public int RequestId { get; set; }
    public DateTime DeliveryDate { get; set; }
    public string DeliveryLocation { get; set; } = "";
    public List<MenuProductInypodDto> Products { get; set; }
    
    public int DeliveryTimeId { get; set; }
    public decimal DeliveryAmount { get; set; }
}