using System;

namespace Canteen.Services.Dto.CreateProduct;

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int EstablishmentId { get; set; }

    public ICollection<int> DietaryRestrictionIds { get; set; } = new List<int>();

    public string Ingredients { get; set; } = string.Empty;

    public ICollection<string> ImagesUrl { get; set; } = new List<string>();

}
