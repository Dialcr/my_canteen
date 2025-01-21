namespace Canteen.Services.Dto.DietaryRestriction;

public record class DietaryRestrictionDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
}
