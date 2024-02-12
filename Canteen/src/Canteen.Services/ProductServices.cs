using Canteen.DataAccess;
using Canteen.Services.Dto;
using Microsoft.EntityFrameworkCore;
using OneOf;

namespace Canteen.Services;

public class ProductServices : CustomServiceBase
{
    public ProductServices(EntitiesContext context)
        : base(context)
    {
    }

    public OneOf<ResponseErrorDto, List<Product>> GetCantneeProductsByCategory(string categoryProduct)
    {
        var result = _context.Products.Where(x => x.Category == categoryProduct).ToList();

        if (result is null)
        {
            return new ResponseErrorDto()
            {
                Status = 404,
                Title = "Products not found",
                Detail = $"The product with category {categoryProduct} has not been found"
            };
        }

        return result;
    }

    public async Task<OneOf<ResponseErrorDto, List<DayProduct>>> GetCantneeProductsByMenu(Menu dayMenu)
    {
        var result = dayMenu.ProductsDay
            .Where(x => x.Quantity > 0)
            .ToList();

        if (result is null)
        {
            return new ResponseErrorDto()
            {
                Status = 404,
                Title = "Menu have not products",
                Detail = $"The menu with id {dayMenu.Id} of the establishment with id {dayMenu.IdEstablishment} have not products"
            };
        }

        return result;
    }

    public OneOf<ResponseErrorDto, Product> GetCantneeProductById(int productId)
    {
        var result = _context.Products
            .SingleOrDefault(x => x.Id == productId);

        if (result is null)
        {
            return new ResponseErrorDto()
            {
                Status = 404,
                Title = "Product not found",
                Detail = $"The product with id {productId} has not been found"
            };
        }

        return result;
    }

    public OneOf<ResponseErrorDto, List<Product>> GetCantneeProductsByDietaryRestrictions(string dietaryRestriction)
    {
        var result = _context.Products
            .Where(x => x.DietaryRestrictions.Contains(dietaryRestriction))
            .ToList();

        if (result is null)
        {
            return new ResponseErrorDto()
            {
                Status = 404,

                Title = "Products not found",
                Detail = $"The product with dietary restriction {dietaryRestriction} has not been found"
            };
        }

        return result;
    }
}
