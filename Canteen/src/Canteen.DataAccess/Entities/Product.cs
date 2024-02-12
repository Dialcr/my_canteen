namespace Canteen.DataAccess.Entities;

public class Product
{
    [Key]
    public int Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; }

    [MaxLength(255)]
    public string Description { get; set; }

    [MaxLength(50)]
    public string Category { get; set; }

    public decimal Price { get; set; }

    [Required]
    public int EstablishmentId { get; set; }

    [ForeignKey(nameof(EstablishmentId))]
    public Establishment? Establishment { get; set; }

    public int Quantity { get; set; }

    public ICollection<string>? DietaryRestrictions { get; set; }
    public ICollection<Request>? Requests { get; set; }
    public ICollection<string>? Ingredients { get; set; }
}