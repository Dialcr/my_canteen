using Canteen.DataAccess;
using Canteen.DataAccess.Enums;
using Canteen.Services.Dto;
using OneOf;
namespace Canteen.Services.Services;

public class CartServices : CustomServiceBase
{
    private readonly CanteenOrderServices _orderServices;
    public CartServices(EntitiesContext context, CanteenOrderServices orderServices) : base(context)
    {
        _orderServices = orderServices;
    }

    public async Task<OneOf<ResponseErrorDto, Order>> Checkout(int cartId)
    {
        var cart = await _context.Carts
            .Include(x => x.Requests)
            .SingleOrDefaultAsync(x => x.Id == cartId);
        //todo: implemet payment method
        
        if (cart is null)
        {
            return new ResponseErrorDto()
            {
                Status = 404,
                Title = "Cart not found",
                Detail = $"The cart with id {cartId} has not found"
            };
        }

        var resultOrder = await _orderServices.CreateOrder(cart);
        if (resultOrder.TryPickT0(out var error, out var order))
        {
            return error;
        }

        _context.Carts.Remove(cart);
        await _context.SaveChangesAsync();
        return order;
    }
    public async Task<OneOf<ResponseErrorDto, CanteenCart>> ApplyDiscountToCart(
        int cardId)
    {
        var cart = await _context.Carts.FindAsync(cardId);
        if (cart is null)
        {
            return new ResponseErrorDto()
            {
                Status = 404,
                Title = "Cart not found",
                Detail = $"The order with id {cardId} has not found"
            };
        }

        var totalDiscount = _context.Discounts.Where(x => x.Establishment!.Id == cart.EstablishmentId
                                                          && x.DiscountType.Equals(DiscountType.TotalAmount)
                                                          && cart.PrductsTotalAmount <= x.TotalNecesity)
            .OrderByDescending(x => x.TotalNecesity)
            .FirstOrDefault();

        var delivaryDiscount = _context.Discounts.Where(x => x.Establishment!.Id == cart.EstablishmentId
                                                             && x.DiscountType.Equals(DiscountType.DeliveryAmount)
                                                             && cart.DeliveryTotalAmount <= x.TotalNecesity)
            .OrderByDescending(x => x.TotalNecesity)
            .FirstOrDefault();


        cart.ProductTotalDiscount = (totalDiscount is not null) ? totalDiscount.DiscountDecimal : 1;
        cart.DeliveryTotalDiscount = (delivaryDiscount is not null) ? delivaryDiscount.DiscountDecimal : 1;

        if (cart.ProductTotalDiscount == 1)
        {
            return new ResponseErrorDto()
            {
                Status = 404,
                Title = "Discount not found",
                Detail = $"Not total discount available"
            };
        }

        if (cart.DeliveryTotalDiscount == 1)
        {
            return new ResponseErrorDto()
            {
                Status = 404,
                Title = "Discount not found",
                Detail = $"Not delivery discount available"
            };
        }

        await _context.SaveChangesAsync();

        return cart;
    }

    public async Task<OneOf<ResponseErrorDto, CanteenCart>> UpdateTotals(int orderId)
    {
        var cart = await _context.Carts.Include(x => x.Requests).FirstOrDefaultAsync(x => x.Id == orderId);
        if (cart is null)
        {
            return new ResponseErrorDto()
            {
                Status = 404,
                Title = "Order not found",
                Detail = $"The order with id {orderId} has not found"
            };
        }
        cart.PrductsTotalAmount = cart.Requests!.Where(x => x.Status == RequestStatus.Planned)
            .Sum(x => x.TotalAmount);
        cart.DeliveryTotalAmount = cart.Requests!.Where(x => x.Status == RequestStatus.Planned)
            .Sum(x => x.DeliveryAmount);
        await _context.SaveChangesAsync();
        return cart;

    }
    public async Task<OneOf<ResponseErrorDto, CanteenCart>> GetCartByUserId(int userId)
    {
        var cart = await _context.Carts
            .Include(x => x.Requests)
            .FirstOrDefaultAsync(x => x.UserId == userId);
        if (cart is null)
        {
            return new ResponseErrorDto()
            {
                Status = 400,
                Title = "Cart not found",
                Detail = $"The cart of user with id {userId} has not found"
            };
        }
        return cart;
    }

}