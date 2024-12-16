using System;
using Canteen.DataAccess.Enums;
using Canteen.Services.Dto;

namespace Canteen.Services.Abstractions;

public interface IProductServices
{
    Task<OneOf<ResponseErrorDto, ICollection<ProductOutputDto>>> GetCantneeProductsByCategoryAsync(ProductCategory categoryProduct);
    OneOf<ResponseErrorDto, IEnumerable<MenuProduct>> GetCantneeProductsByMenu(Menu dayMenu);
    OneOf<ResponseErrorDto, Product> GetCantneeProductById(int productId);
    OneOf<ResponseErrorDto, ICollection<ProductOutputDto>> GetCantneeProductsByDietaryRestrictions(string dietaryRestriction);

}
