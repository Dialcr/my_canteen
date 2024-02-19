using System.Collections;

namespace Canteen.DataAccess.Entities;

public class Discount
{
    [Key]
    public int Id { get; set; }
    
    public decimal DiscountDecimal  { get; set; }

    public decimal TotalNecesity  { get; set; }

    public int EstablishmentId { get; set; }
    
    [ForeignKey(nameof(EstablishmentId))]
    public Establishment Establishment { get; set; }

    public string DiscountType { get; set; }
    
}