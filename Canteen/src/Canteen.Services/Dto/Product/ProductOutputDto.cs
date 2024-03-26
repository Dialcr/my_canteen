using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Canteen.DataAccess.Enums;

namespace Canteen.Services.Dto;

public class ProductOutputDto
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public ProductCategory Category { get; set; }

    public decimal Price { get; set; }

    public int EstablishmentId { get; set; }

    public ICollection<DietaryRestriction>? DietaryRestrictions { get; set; }
    //public ICollection<string>? Ingredients { get; set; }
    public string Ingredients { get; set; }
    
    public ICollection<ProductImageUrl>? ImagesUrl { get; set; }
}

public static class ProductExtention
{
    public static ProductOutputDto ToProductOutputDto(this Product product)
    {
        
        return new ProductOutputDto()
        {
            EstablishmentId = product.EstablishmentId,
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Category = product.Category,
            Price = product.Price,
            Ingredients = product.Ingredients,
            DietaryRestrictions = product.DietaryRestrictions,
            ImagesUrl = product.ImagesUrl
        };

    }
}