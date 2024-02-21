using System.Collections;
using Canteen.DataAccess;
using Canteen.DataAccess.Enums;
using Canteen.Services.Dto;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Canteen.Services.Services;

public class ProductServices : CustomServiceBase
{
    public ProductServices(EntitiesContext context)
        : base(context)
    {
    }

    public async Task<OneOf<ResponseErrorDto, List<Product>>> GetCantneeProductsByCategory(ProductCategory categoryProduct)
    {
        var result = await _context.Products.Where(x => x.Category == categoryProduct).ToListAsync();

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

    public async Task<OneOf<ResponseErrorDto, IEnumerable<MenuProduct>>> GetCantneeProductsByMenu(Menu dayMenu)
    {
        var result = dayMenu.MenuProducts
            .Where(x => x.Quantity > 0)
            .ToList();
            

        if (result is null)
        {
            return new ResponseErrorDto()
            {
                Status = 404,
                Title = "Menu have not products",
                Detail = $"The menu with id {dayMenu.Id} of the establishment with id {dayMenu.EstablishmentId} have not products"
            };
        }

        return result;
    }

    public OneOf<ResponseErrorDto, Product> GetCantneeProductById(int productId)
    {
        var result = _context.Products.SingleOrDefault(x => x.Id == productId);

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
    

    public IQueryable<Product> GetCantneeProductsByDietaryRestrictions(string dietaryRestriction)
    {
        var result =  _context.Products
                .Where(x => x.DietaryRestrictions!.Any(y => y.Description == dietaryRestriction));
        return result;
    }
}