namespace Canteen.DataAccess.Entities;

public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }

    DateTime? UpdatedAt { get; set; }
}
