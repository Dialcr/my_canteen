using System.Collections;
using Canteen.DataAccess;
using Canteen.DataAccess.Enums;
using Canteen.Services.Dto;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Canteen.Services.Services;

public class ProductServices(EntitiesContext context) : CustomServiceBase(context)
{
    public async Task<OneOf<ResponseErrorDto, ICollection<ProductOutputDto>>> GetCantneeProductsByCategoryAsync(ProductCategory categoryProduct)
    {
        var result = await context.Products.Include(x=>x.DietaryRestrictions)
            .Include(x=>x.ImagesUrl)
            .Where(x => x.Category == categoryProduct).ToListAsync();

        if (result is null)
        {
            return Error("Products not found",
                $"The product with category {categoryProduct} has not been found",
                400);
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
            return Error("Menu have not products",
                $"The menu with id {dayMenu.Id} of the establishment with id {dayMenu.EstablishmentId} have not products",
                400);
        }

        return result;
    }

    public OneOf<ResponseErrorDto, Product> GetCantneeProductById(int productId)
    {
        var result = context.Products.Include(x=>x.DietaryRestrictions)
            .Include(x=>x.ImagesUrl)
            .SingleOrDefault(x => x.Id == productId);

        if (result is null)
        {
            return Error("Product not found",
                $"The product with id {productId} has not been found",
                400);
        }
        return result;
    }
    

    public OneOf<ResponseErrorDto,ICollection<ProductOutputDto>> GetCantneeProductsByDietaryRestrictions(string dietaryRestriction)
    {
        var result =  context.Products.Include(x=>x.DietaryRestrictions)
            .Include(x=>x.ImagesUrl)
            .Where(x => x.DietaryRestrictions!.Any(y => y.Description == dietaryRestriction));
        if (!result.Any())
        {
            return Error("Products not found",
                $"The product with dietary restriction {dietaryRestriction} has not been found",
                400);
        }
        return result.Select(x => x.ToProductOutputDto()).ToList();
    }
}