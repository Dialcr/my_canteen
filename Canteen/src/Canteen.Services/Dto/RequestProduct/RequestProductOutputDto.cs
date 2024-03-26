using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Canteen.DataAccess.Enums;

namespace Canteen.Services.Dto;

public class RequestProductOutputDto
{
    public int Id { get; set; }
    
    public int RequestId { get; set; }
    
    public int ProductId { get; set; }
    
    public string ProuctName { get; set; }

    public string ProductDescription { get; set; }

    public ProductCategory Category { get; set; }

   public decimal ProductPrice { get; set; }

    public int EstablishmentId { get; set; }
    public int Quantity { get; set; }

}


public static class RequestProductExtention
{
    public static RequestProductOutputDto ToRequestProductOutputDto(this RequestProduct requestProduct)
    {
        
        return new RequestProductOutputDto()
        {
           ProductId = requestProduct.ProductId,
           RequestId = requestProduct.RequestId,
           ProuctName = requestProduct.Product.Name,
           ProductDescription = requestProduct.Product.Description,
           Category = requestProduct.Product.Category,
           ProductPrice = requestProduct.Product.Price,
           Id = requestProduct.Id,
           Quantity = requestProduct.Quantity   
            
        };

    }
}