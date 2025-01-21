namespace Canteen.Services.Dto.EstablishmentCategory;

public record class UpdateEstablishmentCategoryDto
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
}
