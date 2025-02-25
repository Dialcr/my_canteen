using System;
using Canteen.DataAccess.Enums;
using Canteen.Services.Dto;
using Canteen.Services.Dto.CreateProduct;

namespace Canteen.Services.Abstractions;

public interface IProductServices
{
    Task<OneOf<ResponseErrorDto, ICollection<ProductOutputDto>>> GetCantneeProductsByCategoryAsync(string categoryProduct);
    OneOf<ResponseErrorDto, IEnumerable<MenuProduct>> GetCantneeProductsByMenu(Menu dayMenu);
    OneOf<ResponseErrorDto, Product> GetCantneeProductById(int productId);
    OneOf<ResponseErrorDto, Product> CreateCanteenProduct(CreateProductDto product);
    OneOf<ResponseErrorDto, ICollection<ProductOutputDto>> GetCantneeProductsByDietaryRestrictions(string dietaryRestriction);
    IEnumerable<Product> GetAllProducts(int? establishmentId);
    OneOf<ResponseErrorDto, Product> UpdateCanteenProduct(int productId, CreateProductDto product);
}
