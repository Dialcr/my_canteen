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

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? CanceledAt { get; set; }

    public decimal PrductsTotalAmount { get; set; }
    public decimal ProductTotalDiscount { get; set; }
    public decimal DeliveryTotalAmount { get; set; }
    public decimal DeliveryTotalDiscount { get; set; }

    public ICollection<CanteenRequest> Requests { get; set; } = [];
    public int UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public AppUser? User { get; set; }
}
