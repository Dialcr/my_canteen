using Canteen.DataAccess.Enums;

namespace Canteen.DataAccess.Entities;

public class Request
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int? OrderId { get; set; }

    [ForeignKey(nameof(OrderId))]
    public Order? Order { get; set; }

    public int UserId { get; set; }

    [Required]
    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public DateTimeOffset DeliveryDate { get; set; }

    [MaxLength(255)]
    public string DeliveryLocation { get; set; }

    public decimal TotalAmount { get; set; }
    public decimal DeliveryAmount { get; set; }

    public ICollection<RequestProduct>? RequestProducts { get; set; }

    [MaxLength(25)]
    public RequestStatus Status { get; set; }
    [Required]
    public int? CartId { get; set; }

    [ForeignKey(nameof(CartId))]
    public CanteenCart? Cart { get; set; }
    
    //todo: add delivery time types 
    /*
     * for example :
     * desayuno 7am-9am
     * almuerzo 12pm-2pm
     * comida 7pm-9pm
     */
    public int DeliveryTimeId { get; set; }
    [ForeignKey(nameof(DeliveryTimeId))]
    public DeliveryTime? DeliveryTime { get; set; }
}