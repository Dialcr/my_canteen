namespace Canteen.DataAccess.Entities;

public class Establishment
{
    [Key]
    public int Id { get; set; }

    [MaxLength(50)]
    public string Name { get; set; }

    [MaxLength(255)]
    public string Image { get; set; }

    [MaxLength(255)]
    public string Description { get; set; }

    public ICollection<Order>? Orders { get; set; }

    public ICollection<Product>? Products { get; set; }

    public ICollection<Menu>? Menus { get; set; }
}