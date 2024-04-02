namespace Canteen.Services.Dto;

public class CreateRequestInputDto
{
    public int Id { get; set; }
    public DateTime DeliveryDate{ get; set; }
    public string DeliveryLocation{ get; set; }="";
    public ICollection<RequestProductDto> RequestProducts{ get; set; }
    public int EstablishmentId{ get; set; } =0;
    public decimal DeliveryAmount { get; set; }
    public int DeliveryTimeId { get; set; }
}