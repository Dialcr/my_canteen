using Canteen.DataAccess.Enums;
using Canteen.Services.Dto.Mapper;

namespace Canteen.Services.Dto;

public class MenuProductOutputDto
{
    public int MenuProductId { get; set; }

    public int Quantity { get; set; }
    
    //product info
    public int ProductId { get; set; }

    public ProductOutputDto? ProductOutputDto { get; set; }

    

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
            ProductOutputDto = menuProduct.Product!.ToProductOutputDto(),
        };

    }
}