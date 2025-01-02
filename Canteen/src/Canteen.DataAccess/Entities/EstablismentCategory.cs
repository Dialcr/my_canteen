using System;
using Canteen.DataAccess.Enums;

namespace Canteen.DataAccess.Entities;

public class EstablishmentCategory
{
    [Key]
    public int Id { get; set; }

    [MaxLength(50)]
    public string Name { get; set; }

    [MaxLength(255)]
    public string? Image { get; set; }

    [MaxLength(255)]
    public string Description { get; set; }
    public StatusBase StatusBase { get; set; } = StatusBase.Active;

    public ICollection<Establishment> Establishments { get; set; } = [];
}
