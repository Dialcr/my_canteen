using System;

namespace Canteen.Services.Dto.EstablishmentCategory;

public class EstablishmentCategoryOutputDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public string StatusBase { get; set; } = string.Empty;


}
public static class EstablishmentCategoryExtention
{
    public static EstablishmentCategoryOutputDto ToEstablishmentCategoryOutputDtos(this DataAccess.Entities.EstablishmentCategory establishmentCategory)
    {

        return new EstablishmentCategoryOutputDto()
        {
            Description = establishmentCategory.Description,
            Id = establishmentCategory.Id,
            Name = establishmentCategory.Name,
            StatusBase = establishmentCategory.StatusBase.ToString()
        };

    }
}