namespace Canteen.DataAccess.Entities;

public class CanteenCart
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int EstablishmentId { get; set; }
    [ForeignKey(nameof(EstablishmentId))]
    public Establishment? Establishment { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public decimal PrductsTotalAmount { get; set; }
    public decimal ProductTotalDiscount { get; set; }
    public decimal DeliveryTotalAmount { get; set; }
    public decimal DeliveryTotalDiscount { get; set; }

    public ICollection<CanteenRequest>? Requests { get; set; }
    
    public int UserId { get; set; }
}