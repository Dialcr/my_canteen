namespace Canteen.Services.Dto;

public class RequestIntpudDto
{
    public DateTime DeliveryDate{ get; set; }
    public string DeliveryLocation{ get; set; }
    public ICollection<RequestProduct> RequestProducts{ get; set; }
    public int EstablishmentId{ get; set; } =0;
    public decimal DeliveryAmount { get; set; }
}