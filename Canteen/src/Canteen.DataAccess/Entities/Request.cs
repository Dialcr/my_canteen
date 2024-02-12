namespace Canteen.DataAccess.Entities;

public class Request
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int OrderId { get; set; }

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

    public ICollection<Product>? Products { get; set; }

    [MaxLength(25)]
    public string Status { get; set; }
}