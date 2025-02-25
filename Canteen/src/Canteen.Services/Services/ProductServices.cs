using System.Collections;
using Canteen.DataAccess;
using Canteen.DataAccess.Enums;
using Canteen.Services.Abstractions;
using Canteen.Services.Dto;
using Canteen.Services.Dto.CreateProduct;
using Canteen.Services.Dto.Mapper;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Canteen.Services.Services;

public class ProductServices(EntitiesContext context) : CustomServiceBase(context), IProductServices
{
    public OneOf<ResponseErrorDto, Product> CreateCanteenProduct(CreateProductDto product)
    {
        if (!System.Enum.TryParse(product.Category, true, out ProductCategory category))
        {
            return Error("Invalid category",
                $"Invalid category",
                400);
        }
        var establishment = context.Establishments.FirstOrDefault(x => x.Id == product.EstablishmentId);
        if (establishment is null)
        {
            return Error("Establishment not found",
                $"Establishment not found",
                400);

        }
        var dietaryRestrictions = context.DietaryRestrictions.Where(x => product.DietaryRestrictionIds.Contains(x.Id));
        if (dietaryRestrictions.Count() != product.DietaryRestrictionIds.Count())
        {
            return Error("Dietary restriction not found",
                $"Dietary restriction not found",
                400);

        }
        var images = product.ImagesUrl.Select(x => new ProductImageUrl
        {
            Url = x
        }).ToList();
        var newProduct = new Product()
        {
            Category = category,
            EstablishmentId = establishment.Id,
            Establishment = establishment,
            Description = product.Description,
            Name = product.Name,
            DietaryRestrictions = dietaryRestrictions.ToList(),
            ImagesUrl = images,
            Ingredients = product.Ingredients,
            Price = product.Price,

        };
        newProduct.DietaryRestrictions = dietaryRestrictions.ToList();
        newProduct.ImagesUrl = images;

        context.Products.Add(newProduct);
        context.SaveChanges();
        return newProduct;
    }

    public OneOf<ResponseErrorDto, Product> UpdateCanteenProduct(int productId, CreateProductDto product)
    {
        var existingProduct = context.Products
            .Include(p => p.DietaryRestrictions)
            .Include(p => p.ImagesUrl)
            .FirstOrDefault(p => p.Id == productId);

        if (existingProduct == null)
        {
            return Error("Product not found", $"Product with ID {productId} not found", 400);
        }

        if (!System.Enum.TryParse(product.Category, true, out ProductCategory category))
        {
            return Error("Invalid category", "Invalid category", 400);
        }

        var establishment = context.Establishments.FirstOrDefault(x => x.Id == product.EstablishmentId);
        if (establishment is null)
        {
            return Error("Establishment not found", "Establishment not found", 400);
        }

        var dietaryRestrictions = context.DietaryRestrictions.Where(x => product.DietaryRestrictionIds.Contains(x.Id)).ToList();
        if (dietaryRestrictions.Count != product.DietaryRestrictionIds.Count)
        {
            return Error("Dietary restriction not found", "Dietary restriction not found", 400);
        }

        var images = product.ImagesUrl.Select(x => new ProductImageUrl { Url = x }).ToList();

        existingProduct.Category = category;
        existingProduct.EstablishmentId = establishment.Id;
        existingProduct.Description = product.Description;
        existingProduct.Name = product.Name;
        existingProduct.DietaryRestrictions = dietaryRestrictions;
        existingProduct.ImagesUrl = images;
        existingProduct.Ingredients = product.Ingredients;
        existingProduct.Price = product.Price;

        context.SaveChanges();
        return existingProduct;
    }

    public async Task<OneOf<ResponseErrorDto, ICollection<ProductOutputDto>>> GetCantneeProductsByCategoryAsync(string categoryProduct)
    {
        if (!System.Enum.TryParse(categoryProduct, out ProductCategory category))
        {

        }
        var result = await context.Products.Include(x => x.DietaryRestrictions)
            .Include(x => x.ImagesUrl)
            .Where(x => x.Category == category).ToListAsync();

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
        var result = context.Products.Include(x => x.DietaryRestrictions)
            .Include(x => x.ImagesUrl)
            .SingleOrDefault(x => x.Id == productId);

        if (result is null)
        {
            return Error("Product not found",
                $"The product with id {productId} has not been found",
                400);
        }
        return result;
    }
    public IEnumerable<Product> GetAllProducts(int? establishmentId)
    {
        var result = context.Products
            .Include(x => x.DietaryRestrictions)
            .Include(x => x.ImagesUrl)
            .Where(x => establishmentId == null || x.EstablishmentId == establishmentId).ToList();

        return result;
    }


    public OneOf<ResponseErrorDto, ICollection<ProductOutputDto>> GetCantneeProductsByDietaryRestrictions(string dietaryRestriction)
    {
        var result = context.Products.Include(x => x.DietaryRestrictions)
            .Include(x => x.ImagesUrl)
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