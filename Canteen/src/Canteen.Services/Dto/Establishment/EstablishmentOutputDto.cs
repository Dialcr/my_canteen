using Canteen.DataAccess.Entities;
using Canteen.Services.Dto.EstablishmentCategory;
namespace Canteen.Services.Dto.Establishment;
public class EstablishmentOutputDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Image { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    public string StatusBase { get; set; } = string.Empty;
    public IEnumerable<EstablishmentCategoryOutputDto> Categories { get; set; } = [];



}

public static class EstablishmentExtention
{
    public static EstablishmentOutputDto ToEstablishmentOutputDtos(this DataAccess.Entities.Establishment establishment)
    {
        var categories = establishment.EstablishmentCategories.Select(x => new EstablishmentCategoryOutputDto
        {
            Name = x.Name,
            Description = x.Description,
            Id = x.Id,
            StatusBase = x.StatusBase.ToString()
        }).ToList();
        return new EstablishmentOutputDto()
        {
            Description = establishment.Description,
            Id = establishment.Id,
            Image = establishment.Image,
            Name = establishment.Name,
            StatusBase = establishment.StatusBase.ToString(),
            Categories = categories
        };

    }
}