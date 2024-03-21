namespace Canteen.DataAccess.Entities;

public class MenuProduct
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CanteenProductId { get; set; }

    [ForeignKey(nameof(CanteenProductId))]
    public Product? Product { get; set; }

    [Required]
    public int Quantity { get; set; }

    [Required]
    public int MenuId { get; set; }

    [ForeignKey(nameof(MenuId))]
    public Menu? Menu { get; set; }
}