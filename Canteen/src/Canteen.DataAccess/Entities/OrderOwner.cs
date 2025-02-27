namespace Canteen.DataAccess.Entities;

public class OrderOwner
{
    [Key]
    public int Id { get; set; }

    public string Identifier { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}
