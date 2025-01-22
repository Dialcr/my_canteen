using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Canteen.DataAccess.Enums;
using Canteen.Services.Dto.DietaryRestriction;
using Canteen.Services.Dto.Images;

namespace Canteen.Services.Dto;

public class ProductOutputDto
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public ProductCategory Category { get; set; }

    public decimal Price { get; set; }

    public int EstablishmentId { get; set; }

    public IEnumerable<DietaryRestrictionDto>? DietaryRestrictions { get; set; }
    //public ICollection<string>? Ingredients { get; set; }
    public string Ingredients { get; set; }

    public IEnumerable<ImagesDto>? ImagesUrl { get; set; }
}

public static class ProductExtention
{
    public static ProductOutputDto ToProductOutputDtos(this Product product)
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
            DietaryRestrictions = product.DietaryRestrictions.Select(x => new DietaryRestrictionDto
            {
                Id = x.Id,
                Description = x.Description
            }),
            ImagesUrl = product.ImagesUrl.Select(x => new ImagesDto
            {
                Id = x.Id,
                Url = x.Url ?? string.Empty
            })
        };

    }
}