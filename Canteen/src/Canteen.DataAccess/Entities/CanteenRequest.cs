using Canteen.DataAccess.Enums;

namespace Canteen.DataAccess.Entities;

public class CanteenRequest
{
    [Key]
    public int Id { get; set; }

    public int? OrderId { get; set; }

    [ForeignKey(nameof(OrderId))]
    public Order? Order { get; set; }

    public int UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public AppUser? User { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime DeliveryDate { get; set; }

    [MaxLength(255)]
    public string DeliveryLocation { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }
    public decimal DeliveryAmount { get; set; }

    public ICollection<RequestProduct>? RequestProducts { get; set; }

    public RequestStatus Status { get; set; }

    public int? CartId { get; set; }

    [ForeignKey(nameof(CartId))]
    public CanteenCart? Cart { get; set; }
    public int DeliveryTimeId { get; set; }
    [ForeignKey(nameof(DeliveryTimeId))]
    public DeliveryTime? DeliveryTime { get; set; }
}