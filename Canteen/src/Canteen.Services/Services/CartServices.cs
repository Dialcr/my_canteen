using Canteen.DataAccess;
using Canteen.DataAccess.Enums;
using Canteen.Services.Abstractions;
using Canteen.Services.Dto;
using Canteen.Services.Dto.CanteenRequest;
using Canteen.Services.Dto.Order;

namespace Canteen.Services.Services;

public class CartServices(EntitiesContext context, ICanteenOrderServices orderServices, IRequestServices requestServices) : CustomServiceBase(context), ICartServices
{
    public async Task<OneOf<IEnumerable<RequestProductOutputDto>, OrderOutputDto>> CheckoutAsync(int cartId)
    {
        var cart = await context.Carts
            .Include(x => x.Requests)
            .ThenInclude(x => x.RequestProducts)
            .ThenInclude(requestProduct => requestProduct.Product)
            .SingleOrDefaultAsync(x => x.Id == cartId);

        var productsOutput = new List<RequestProductOutputDto>();
        if (cart is null)
        {
            return productsOutput;
        }
        foreach (var request in cart.Requests!)
        {
            var menu = await context.Menus.Include(x => x.MenuProducts)
                .FirstOrDefaultAsync(x => x.EstablishmentId == cart.EstablishmentId
                                        && x.Date.Date == request.DeliveryDate.Date);
            if (menu is null)
            {


                productsOutput = productsOutput
                    .Concat(request.RequestProducts
                        .Select(x => x.ToRequestProductOutputDto())).ToList();
            }
            else
            {
                var result = requestServices.AllProductsOk(request, menu!);
                if (result is not null)
                {
                    productsOutput = productsOutput.Concat(result).ToList();
                }
            }

        }

        if (productsOutput.Count > 0)
        {
            return productsOutput;
        }

        await Task.WhenAll(cart.Requests.ToList().Select(request => requestServices.DiscountFromInventaryAsync(request, cart.EstablishmentId)));
        var order = await orderServices.CreateOrderAsync(cart);

        //todo: implemet payment method

        context.Carts.Remove(cart);
        await context.SaveChangesAsync();
        return order.ToOrderOutputDto();
    }

    public async Task<OneOf<ResponseErrorDto, CanteenCart>> ApplyDiscountToCartAsync(
        int cardId)
    {
        var cart = await context.Carts.FindAsync(cardId);
        if (cart is null)
        {
            return Error(
                "Cart not found",
                $"The order with id {cardId} has not found",
                400
            );
        }

        var totalDiscount = context.Discounts.Where(x => x.Establishment!.Id == cart.EstablishmentId
                                                         && x.DiscountType == DiscountType.TotalAmount
                                                         && cart.PrductsTotalAmount <= x.TotalNecesity)
            .OrderByDescending(x => x.TotalNecesity)
            .FirstOrDefault();

        var delivaryDiscount = context.Discounts.Where(x => x.Establishment!.Id == cart.EstablishmentId
                                                            && x.DiscountType == DiscountType.DeliveryAmount
                                                            && cart.DeliveryTotalAmount <= x.TotalNecesity)
            .OrderByDescending(x => x.TotalNecesity)
            .FirstOrDefault();


        cart.ProductTotalDiscount = (totalDiscount is not null) ? totalDiscount.DiscountDecimal : 1;
        cart.DeliveryTotalDiscount = (delivaryDiscount is not null) ? delivaryDiscount.DiscountDecimal : 1;

        await context.SaveChangesAsync();

        return cart;
    }

    public async Task<OneOf<ResponseErrorDto, CanteenCart>> UpdateTotalsIntoCartAsync(int cartId)
    {
        var cart = await context.Carts.Include(x => x.Requests).FirstOrDefaultAsync(x => x.Id == cartId);
        if (cart is null)
        {
            return Error("Order not found",
                $"The order with id {cartId} has not found",
                400);
        }
        cart.PrductsTotalAmount = cart.Requests!.Where(x => x.Status == RequestStatus.Planned)
            .Sum(x => x.TotalAmount);
        cart.DeliveryTotalAmount = cart.Requests!.Where(x => x.Status == RequestStatus.Planned)
            .Sum(x => x.DeliveryAmount);
        await ApplyDiscountToCartAsync(cartId);
        await context.SaveChangesAsync();
        return cart;

    }
    public async Task<OneOf<ResponseErrorDto, CartOutputDto>> GetCartByUserIdAsync(int userId)
    {
        var cart = await context.Carts
            .Include(x => x.Requests)
            .ThenInclude(x => x.RequestProducts)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.UserId == userId);
        if (cart is null)
        {
            return Error("Cart not found",
                $"The cart of user with id {userId} has not found",
                400);
        }
        return cart.ToCanteenCartDto();
    }

    public async Task<OneOf<ResponseErrorDto, RequestOutputDto>> EditRequestIntoCartAsync(
        EditRequestDto requestDto)
    {

        var request = await context.Requests
            .Include(r => r.RequestProducts)!
            .ThenInclude(requestProduct => requestProduct.Product)
            .FirstOrDefaultAsync(r => r.Id == requestDto.RequestId);

        if (request is null)
        {
            return Error("Request not found",
                $"The request with id {requestDto.RequestId} was not found",
                400);
        }
        if (!request.Status.Equals(RequestStatus.Planned))
        {
            return Error("Invalid request status",
                "The request status has changed and cannot be edited",
                400);
        }

        request.RequestProducts ??= new List<RequestProduct>();


        foreach (var productDto in requestDto.Products)
        {
            var existingProduct = request.RequestProducts.FirstOrDefault(p => p.ProductId == productDto.ProductId);
            if ((existingProduct is not null))
            {

                existingProduct.Quantity = productDto.Quantity;

            }
            else
            {
                var addProduct = context.Products.FirstOrDefault(x => x.Id == productDto.ProductId);
                if (addProduct is null)
                {
                    return Error("Product not found",
                        $"The product with id {productDto.ProductId} has not been found",
                        400);
                }
                request.RequestProducts.Add(new RequestProduct()
                {
                    ProductId = productDto.ProductId,
                    RequestId = request.Id,
                    Quantity = productDto.Quantity,
                    UnitPrice = addProduct.Price
                });
            }
        }
        request.DeliveryDate = requestDto.DeliveryDate;
        request.DeliveryLocation = requestDto.DeliveryLocation;
        request.TotalAmount = request.RequestProducts!.Sum(x => x.UnitPrice * x.Quantity);
        request.DeliveryAmount = requestDto.DeliveryAmount;
        request.DeliveryTimeId = requestDto.DeliveryTimeId;
        await UpdateTotalsIntoCartAsync(request.CartId!.Value);
        await ApplyDiscountToCartAsync(request.CartId!.Value);
        await context.SaveChangesAsync();

        return request.ToCanteenRequestWithProductsDto();
    }
    public async Task<OneOf<ResponseErrorDto, CanteenRequest>> AddProductToRequestCartAsync(
        int userId,
        RequestInputDto dto)
    {

        var request = await context.Requests
            .Include(r => r.RequestProducts)!
            .FirstOrDefaultAsync(r => r.Id == dto.RequestId && r.UserId == userId);

        if (request is null)
        {
            return Error("Request not found",
                $"The request with id {dto.RequestId} was not found",
                400);
        }

        if (request.RequestProducts is null)
        {
            request.RequestProducts = new List<RequestProduct>();
        }
        var requestProduct = request.RequestProducts
            .FirstOrDefault(r => r.Id == dto.Product.ProductId && r.RequestId == dto.RequestId);
        if (requestProduct is not null)
        {
            requestProduct.Quantity = dto.Product.Quantity;
        }
        else
        {

            request.RequestProducts.Add(new RequestProduct()
            {
                RequestId = dto.RequestId,
                Quantity = dto.Product.Quantity,
                ProductId = dto.Product.ProductId
            });
        }

        await context.SaveChangesAsync();
        return request;
    }
    //todo: make delete requestPorduct to cart
    public async Task<OneOf<ResponseErrorDto, RequestOutputDto>> DeleteRequestIntoCartAsync(
        int userId,
        int cartId,
        int requestId)
    {
        var cart = await context.Carts.Include(x => x.Requests)
            .FirstOrDefaultAsync(x => x.Id == cartId && x.UserId == userId);
        if (cart is null)
        {
            return Error(" Cart not found",
                $"The cart of user with id {userId} has not found",
                400);
        }
        var request = cart.Requests!.FirstOrDefault(x => x.Id == requestId);
        if (request is null)
        {
            return Error("Request not found",
                $"The request with id {requestId} was not found",
                400);
        }

        context.Requests.Remove(request);
        var affectColumns = await context.SaveChangesAsync();
        if (affectColumns > 0)
        {
            return Error("Failed to delete request",
                $"The request with id {requestId} was not deleted",
                400);
        }

        return request.ToCanteenRequestOutputDto();

    }

    public async Task<OneOf<ResponseErrorDto, CanteenRequest>> PlanningRequestIntoCartAsync(
        int requestId,
        DateTime newDateTime)
    {

        var request = await context.Requests.Include(x => x.RequestProducts).FirstOrDefaultAsync(x => x.Id == requestId);
        if (request is null)
        {
            return Error("Request not found",
                $"The request with id {requestId} was not found",
                400);
        }

        var newRequest = new CanteenRequest()
        {
            CreatedAt = DateTime.Now,
            DeliveryDate = newDateTime,
            DeliveryLocation = request.DeliveryLocation,
            Status = RequestStatus.Planned,
            UserId = request.UserId,
            RequestProducts = request.RequestProducts!.Select(x =>
                new RequestProduct()
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice
                }).ToList(),
            TotalAmount = request.TotalAmount,
            CartId = request.CartId,
            DeliveryAmount = request.DeliveryAmount,
            DeliveryTimeId = request.DeliveryTimeId,

        };

        await context.Requests.AddAsync(newRequest);


        await context.SaveChangesAsync();
        await UpdateTotalsIntoCartAsync(newRequest.CartId.Value);
        await ApplyDiscountToCartAsync(newRequest.CartId.Value);

        return newRequest;
    }
}