using Canteen.DataAccess.Enums;

namespace Canteen.DataAccess.Entities;

public class DeliveryTime
{
    [Key]
    public int Id { get; set; }

    public int  EstablishmentId { get; set; }
    
    [ForeignKey(nameof(EstablishmentId))]
    public Establishment? Establishment { get; set; }
    public TimeSpan StartTime { get; set; } // sample: 7:00 AM
    public TimeSpan EndTime { get; set; } // sample: 9:00 AM
    public DeliveryTimeType DeliveryTimeType { get; set; }

}