namespace Canteen.Services.Dto;

public class EditRequestDto
{
    public DateTime DeliveryDate { get; set; }
    public string DeliveryLocation { get; set; } = "";
    public List<MenuProductInypodDto> Products { get; set; }
}