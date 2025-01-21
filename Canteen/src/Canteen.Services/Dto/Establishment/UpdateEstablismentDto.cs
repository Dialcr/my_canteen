namespace Canteen.Services.Dto.Establishment;

public record class UpdateEstablismentDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Address { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; } = string.Empty;
    public ICollection<DeliveryTimeDto> DeliveryTimes { get; set; } = [];
    public ICollection<int> EstablishmentCategory { get; set; } = [];

}
