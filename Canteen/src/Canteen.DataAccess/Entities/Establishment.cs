using Canteen.DataAccess.Enums;

namespace Canteen.DataAccess.Entities;

public class Establishment
{
    [Key]
    public int Id { get; set; }

    [MaxLength(50)]
    public string Name { get; set; }

    [MaxLength(255)]
    public string? Image { get; set; }

    [MaxLength(255)]
    public string? Description { get; set; } = string.Empty;
    public StatusBase StatusBase { get; set; } = StatusBase.Active;
    public string? Address { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; } = string.Empty;
    public ICollection<Order> Orders { get; set; } = [];

    public ICollection<Product> Products { get; set; } = [];

    public ICollection<Menu> Menus { get; set; } = [];

    public ICollection<Discount> Discounts { get; set; } = [];

    public ICollection<DeliveryTime> DeliveryTimes { get; set; } = [];
    public ICollection<EstablishmentCategory> EstablishmentCategories { get; set; } = [];


}