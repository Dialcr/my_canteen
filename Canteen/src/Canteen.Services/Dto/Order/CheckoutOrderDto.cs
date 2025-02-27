//crea una clase CheckoutOrderDto
namespace Canteen.Services.Dto.Order;

public class CheckoutOrderDto
{
    public int OrderId { get; set; }
    public OrderOwnerDto OrderOwnner { get; set; } = new OrderOwnerDto();
}