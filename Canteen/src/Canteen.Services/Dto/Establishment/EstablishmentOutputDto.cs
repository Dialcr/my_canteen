using Canteen.DataAccess.Entities;
namespace Canteen.Services.Dto.Establishment;
public class EstablishmentOutputDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Image { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    public string StatusBase { get; set; } = string.Empty;


}

public static class EstablishmentExtention
{
    public static EstablishmentOutputDto ToEstablishmentOutputDtos(this DataAccess.Entities.Establishment establishment)
    {

        return new EstablishmentOutputDto()
        {
            Description = establishment.Description,
            Id = establishment.Id,
            Image = establishment.Image,
            Name = establishment.Name,
            StatusBase = establishment.StatusBase.ToString()

        };

    }
}