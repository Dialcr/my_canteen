namespace Canteen.Services.Dto.EstablishmentCategory;

public record class CreateEstablishmentCategoryDto
{
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Image { get; set; }

}
