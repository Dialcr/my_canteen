namespace Canteen.DataAccess.Entities;

public class DietaryRestriction
{
    [Key] public int Id { get; set; }
    [Required] 
    [MaxLength(100)]
    public string Description { get; set; }

    public ICollection<Product>? Products { get; set; }
}