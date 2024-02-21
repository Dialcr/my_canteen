using Canteen.DataAccess.Enums;

namespace Canteen.DataAccess.Entities;

public class Order : IAuditableEntity
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int EstablishmentId { get; set; }

    [ForeignKey(nameof(EstablishmentId))]
    public Establishment? Establishment { get; set; }

    [MaxLength(50)]
    public OrderStatus Status { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? CanceledAt { get; set; }

    public decimal PrductsTotalAmount { get; set; }
    public decimal ProductTotalDiscount { get; set; }
    public decimal DeliveryTotalAmount { get; set; }
    public decimal DeliveryTotalDiscount { get; set; }

    public ICollection<Request>? Requests { get; set; }
    public int UserId { get; set; }
}
