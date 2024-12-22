using Canteen.DataAccess.Enums;

namespace Canteen.DataAccess.Entities;

public class Product
{
    [Key] public int Id { get; set; }

    [MaxLength(100)]
    public string Name { get; set; }

    [MaxLength(255)]
    public string Description { get; set; }

    [MaxLength(50)]
    public ProductCategory Category { get; set; }

    [Required]
    public decimal Price { get; set; }

    [Required]
    public int EstablishmentId { get; set; }

    [ForeignKey(nameof(EstablishmentId))]
    public Establishment? Establishment { get; set; }

    public ICollection<DietaryRestriction> DietaryRestrictions { get; set; } = [];
    public string Ingredients { get; set; }
    public ICollection<ProductImageUrl>? ImagesUrl { get; set; }

}