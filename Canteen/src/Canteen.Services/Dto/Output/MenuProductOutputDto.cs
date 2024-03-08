using Canteen.DataAccess.Enums;

namespace Canteen.Services.Dto;

public class MenuProductOutputDto
{
    public int MenuProductId { get; set; }

    public int Quantity { get; set; }
    
    //product info
    public int ProductId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public ProductCategory Category { get; set; }

    public decimal Price { get; set; }

    public ICollection<DietaryRestriction>? DietaryRestrictions { get; set; }
    
    public string Ingredients { get; set; }
    
    public ICollection<ProductImageUrl>? ImagesUrl { get; set; }

}


public static class MenuProductExtention
{
    public static MenuProductOutputDto ToEstablishmentOutputDtoWithProducts(this MenuProduct menuProduct)
    {
        
        return new MenuProductOutputDto()
        {
            MenuProductId = menuProduct.Id,
            Quantity = menuProduct.Quantity,
            ProductId = menuProduct.Id,
            Name = menuProduct.Product!.Name,
            Description = menuProduct.Product.Description,
            Category = menuProduct.Product.Category,
            Price = menuProduct.Product.Price,
            DietaryRestrictions = menuProduct.Product.DietaryRestrictions,
            Ingredients = menuProduct.Product.Ingredients,
            ImagesUrl = menuProduct.Product.ImagesUrl
            
            
        };

    }
}