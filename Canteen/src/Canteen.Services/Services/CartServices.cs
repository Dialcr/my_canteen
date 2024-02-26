using Canteen.DataAccess;
using Canteen.DataAccess.Enums;
using Canteen.Services.Dto;
using OneOf;
namespace Canteen.Services.Services;

public class CartServices : CustomServiceBase
{
    private readonly CanteenOrderServices _orderServices;
    private readonly RequestServices _requestServices;
    public CartServices(EntitiesContext context, CanteenOrderServices orderServices, RequestServices requestServices) : base(context)
    {
        _orderServices = orderServices;
        _requestServices = requestServices;
    }

    public async Task<OneOf<ResponseErrorDto,ICollection<RequestInputDto>, Order>> Checkout(int cartId)
    {
        var cart = await _context.Carts
            .Include(x => x.Requests)
            .SingleOrDefaultAsync(x => x.Id == cartId);
        
        if (cart is null)
        {
            return new ResponseErrorDto()
            {
                Status = 404,
                Title = "Cart not found",
                Detail = $"The cart with id {cartId} has not found"
            };
        }
        var productsOutput  = new List<RequestInputDto>(); 
        foreach (var request in cart.Requests!)
        {
            var menu = await _context.Menus.FirstOrDefaultAsync(x=>x.EstablishmentId == cart.EstablishmentId 
                                                                   && x.Date == request.DeliveryDate);
            var result =  _requestServices.AllProductsOk(request, menu!);
            if (result.TryPickT0(out var errorProducts, out var _))
            {
                productsOutput = productsOutput.Concat(errorProducts).ToList();
            }
            
        }
        
        if (productsOutput.Count >0)
        {
            return productsOutput;
        }
        
        foreach (var request in cart.Requests)
        {
            var result = await _requestServices.DiscountFromInventary(request, cart.EstablishmentId);

            if (result.TryPickT0(out var errorInventary, out _))
            {
                return errorInventary;
            }
        }
        var resultOrder = await _orderServices.CreateOrder(cart);
        if (resultOrder.TryPickT0(out var error, out var order))
        {
            return error;
        }
        //todo: implemet payment method

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

    public async Task<OneOf<ResponseErrorDto, CanteenCart>> UpdateTotalsIntoCart(int orderId)
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
    
    public async Task<OneOf<ResponseErrorDto, Request>> EditRequestIntoCart(
        int requestId,
        DateTime deliveryDate,
        string deliveryLocation,
        ICollection<MenuProductInypodDto> productDayDtos)
    {
       
        var request = await _context.Requests
            .Include(r => r.RequestProducts)!
            .ThenInclude(requestProduct => requestProduct.Product)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request is null)
        {
            return new ResponseErrorDto
            {
                Status = 404,
                Title = "Request not found",
                Detail = $"The request with id {requestId} was not found"
            };
        }
        if (!request.Status.Equals(RequestStatus.Planned))
        {
            return new ResponseErrorDto
            {
                Status = 400,
                Title = "Invalid request status",
                Detail = "The request status has changed and cannot be edited"
            };
        }
        
        request.RequestProducts ??= new List<RequestProduct>();
        
       
        foreach (var product in productDayDtos)
        {
            var existingProduct = request.RequestProducts.FirstOrDefault(p => p.ProductId == product.ProductId);
            if ((existingProduct is not null) )
            {
                if (existingProduct.Quantity != product.Quantity)
                {
                    existingProduct.Quantity = product.Quantity;
                    
                }
            }
            else
            {
                request.RequestProducts.Add(new RequestProduct()
                {
                    ProductId = product.ProductId,
                    RequestId = request.Id,
                    Quantity = product.Quantity
                });
            }
        }
        request.DeliveryDate = deliveryDate;
        request.DeliveryLocation = deliveryLocation;
        request.TotalAmount = request.RequestProducts!.Sum(x=>x.Product.Price);
        await UpdateTotalsIntoCart(request.OrderId!.Value);
        await ApplyDiscountToCart(request.OrderId!.Value);
        await _context.SaveChangesAsync();

        return request;
    }
    public async Task<OneOf<ResponseErrorDto, Request>> AddProductToRequestCartAsync(
        int userId,
        RequestInputDto dto)
    {
        
        var request = await _context.Requests
            .Include(r => r.RequestProducts)!
            .FirstOrDefaultAsync(r => r.Id == dto.RequestId && r.UserId == userId);
            
        if (request is null)
        {
            return new ResponseErrorDto()
            {
                Status = 404,
                Title = "Request not found",
                Detail = $"Request '{dto.RequestId}' not found",
            };
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
            
        await _context.SaveChangesAsync();
        return request;
    }
    //todo: make delete requestPorduct to cart
    
    public async Task<OneOf<ResponseErrorDto, Request>> PlanningRequestIntoCart(
        int requestId,
        DateTime newDateTime
        ,int cartId)
    {
       
        var request = await _context.Requests.Include(x=> x.RequestProducts).FirstOrDefaultAsync(x=>x.Id==requestId);
        if (request is null)
        {
            return new ResponseErrorDto()
            {
                Status  = 404,
                Title = "Request not found",
                Detail = $"The request with id {requestId} was not found"
            };
        }

        var newRequest = new Request()
        {
            CreatedAt = DateTime.Now,
            DeliveryDate = newDateTime,
            DeliveryLocation = request.DeliveryLocation,
            Status = RequestStatus.Planned,
            UserId = request.UserId,
            RequestProducts = request.RequestProducts,
            TotalAmount = request.TotalAmount,
            CartId = cartId,
            DeliveryAmount = request.DeliveryAmount,


        };
        await _context.Requests.AddAsync(newRequest);
        
        await ApplyDiscountToCart(request.CartId!.Value);
        
        await _context.SaveChangesAsync();

        return newRequest;
    }

}