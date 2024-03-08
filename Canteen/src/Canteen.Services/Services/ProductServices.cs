using System.Collections;
using Canteen.DataAccess;
using Canteen.DataAccess.Enums;
using Canteen.Services.Dto;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Canteen.Services.Services;

public class ProductServices : CustomServiceBase
{
    public ProductServices(EntitiesContext context)
        : base(context)
    {
    }

    public async Task<OneOf<ResponseErrorDto, ICollection<ProductOutputDto>>> GetCantneeProductsByCategory(ProductCategory categoryProduct)
    {
        var result = await _context.Products.Include(x=>x.DietaryRestrictions)
            .Include(x=>x.ImagesUrl)
            .Where(x => x.Category == categoryProduct).ToListAsync();

        if (result is null)
        {
            return new ResponseErrorDto()
            {
                Status = 404,
                Title = "Products not found",
                Detail = $"The product with category {categoryProduct} has not been found"
            };
        }

        return result.Select(x => x.ToProductOutputDto()).ToList();
    }

    public OneOf<ResponseErrorDto, IEnumerable<MenuProduct>> GetCantneeProductsByMenu(Menu dayMenu)
    {
        var result = dayMenu.MenuProducts!
            .Where(x => x.Quantity > 0)
            .ToList();
            

        if (!result.Any())
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
        var result = _context.Products.Include(x=>x.DietaryRestrictions)
            .Include(x=>x.ImagesUrl)
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
    

    public OneOf<ResponseErrorDto,ICollection<ProductOutputDto>> GetCantneeProductsByDietaryRestrictions(string dietaryRestriction)
    {
        var result =  _context.Products.Include(x=>x.DietaryRestrictions)
            .Include(x=>x.ImagesUrl)
                .Where(x => x.DietaryRestrictions!.Any(y => y.Description == dietaryRestriction));
        if (!result.Any())
        {
            return new ResponseErrorDto()
            {
                Status = 404,
                Title = "Products not found",
                Detail = $"The product with dietary restriction {dietaryRestriction} has not been found"
            };
        }
        return result.Select(x => x.ToProductOutputDto()).ToList();
    }
}