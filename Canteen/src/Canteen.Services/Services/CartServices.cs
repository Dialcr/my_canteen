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

    public async Task<OneOf<ResponseErrorDto,ICollection<RequestInputDto>, OrderOutputDto>> Checkout(int cartId)
    {
        var cart = await _context.Carts
            .Include(x => x.Requests)
            .ThenInclude(x => x.RequestProducts)
            .ThenInclude(requestProduct => requestProduct.Product)
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
            var menu = await _context.Menus.Include(x=>x.MenuProducts)
                .FirstOrDefaultAsync(x=>x.EstablishmentId == cart.EstablishmentId 
                                        && x.Date.Date == request.DeliveryDate.Date);
            //&&  DateTimeOffset.Compare(x.Date, request.DeliveryDate) == 0);
            if (menu is null)
            {
                productsOutput = productsOutput.Concat(request.RequestProducts.Select(x => new RequestInputDto()
                {
                    RequestId = request.Id,
                    Product = new ProductDayDto
                    {
                        Product = x.Product,
                        Quantity = x.Quantity,
                        ProductId = x.ProductId
                    }
                })).ToList();
            }
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
        return order.ToOrderOutputDto();
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
        
        await _context.SaveChangesAsync();

        return cart;
    }

    public async Task<OneOf<ResponseErrorDto, CanteenCart>> UpdateTotalsIntoCart(int cartId)
    {
        var cart = await _context.Carts.Include(x => x.Requests).FirstOrDefaultAsync(x => x.Id == cartId);
        if (cart is null)
        {
            return new ResponseErrorDto()
            {
                Status = 404,
                Title = "Order not found",
                Detail = $"The order with id {cartId} has not found"
            };
        }
        cart.PrductsTotalAmount = cart.Requests!.Where(x => x.Status == RequestStatus.Planned)
            .Sum(x => x.TotalAmount);
        cart.DeliveryTotalAmount = cart.Requests!.Where(x => x.Status == RequestStatus.Planned)
            .Sum(x => x.DeliveryAmount);
        await ApplyDiscountToCart(cartId);
        await _context.SaveChangesAsync();
        return cart;

    }
    public async Task<OneOf<ResponseErrorDto, CartOutputDto>> GetCartByUserId(int userId)
    {
        var cart = await _context.Carts
            .Include(x => x.Requests)
            .ThenInclude(x=>x.RequestProducts)
            .ThenInclude(x=>x.Product)
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
        return cart.ToCanteenCartDto();
    }
    
    public async Task<OneOf<ResponseErrorDto, RequestOutputDto>> EditRequestIntoCart(
        EditRequestDto requestDto)
    {
       
        var request = await _context.Requests
            .Include(r => r.RequestProducts)!
            .ThenInclude(requestProduct => requestProduct.Product)
            .FirstOrDefaultAsync(r => r.Id == requestDto.RequestId);

        if (request is null)
        {
            return new ResponseErrorDto
            {
                Status = 404,
                Title = "Request not found",
                Detail = $"The request with id {requestDto.RequestId} was not found"
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
        
       
        foreach (var productDto in requestDto.Products)
        {
            var existingProduct = request.RequestProducts.FirstOrDefault(p => p.ProductId == productDto.ProductId);
            if ((existingProduct is not null) )
            {
                
                existingProduct.Quantity = productDto.Quantity;
                
            }
            else
            {
                var addProduct = _context.Products.FirstOrDefault(x => x.Id == productDto.ProductId);
                if (addProduct is null)
                {
                    return new ResponseErrorDto()
                    {
                        Status = 404,
                        Title = "Product not found",
                        Detail = $"The product with id {productDto.ProductId} has not been found"
                    };
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
        request.TotalAmount = request.RequestProducts!.Sum(x=>x.UnitPrice * x.Quantity);
        request.DeliveryAmount = requestDto.DeliveryAmount;
        request.DeliveryTimeId = requestDto.DeliveryTimeId;
        await UpdateTotalsIntoCart(request.CartId!.Value);
        await ApplyDiscountToCart(request.CartId!.Value);
        await _context.SaveChangesAsync();

        return request.ToCanteenRequestWithProductsDto();
    }
    public async Task<OneOf<ResponseErrorDto, CanteenRequest>> AddProductToRequestCartAsync(
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
    public async Task<OneOf<ResponseErrorDto, CanteenRequest>> DeleteRequestIntoCart(
        int userId,
        int cartId,
        int requestId)
    {
        var cart = await _context.Carts.Include(x=>x.Requests)
            .FirstOrDefaultAsync(x=>x.Id==cartId && x.UserId==userId);
        if (cart is null)
        {
            return new ResponseErrorDto()
            {
                Status = 404,
                Title = "Cart not found",
                Detail = $"The cart of user with id {userId} has not found"
            };
        }
        var request = cart.Requests!.FirstOrDefault(x=>x.Id==requestId);
        if (request is null)
        {
            return new ResponseErrorDto()
            {
                Status = 404,
                Title = "Request not found",
                Detail = $"The request with id {requestId} was not found"
            };
        }
        
        _context.Requests.Remove(request);
        var affectColumns =await _context.SaveChangesAsync();
        if (affectColumns > 0)
        {
            return new ResponseErrorDto()
            {
                Status = 400,
                Title = "Failed to delete request",
                Detail = $"The request with id {requestId} was not deleted"
            };
        }

        return new ResponseErrorDto()
        {
            Status = 200,
            Title = "Request deleted successfully",
            Detail = $"The request with id {requestId} was deleted successfully"
                
        };
        
    }

    public async Task<OneOf<ResponseErrorDto, CanteenRequest>> PlanningRequestIntoCart(
        int requestId,
        DateTime newDateTime)
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

        var newRequest = new CanteenRequest()
        {
            CreatedAt = DateTime.Now,
            DeliveryDate = newDateTime,
            DeliveryLocation = request.DeliveryLocation,
            Status = RequestStatus.Planned,
            UserId = request.UserId,
            RequestProducts = request.RequestProducts!.Select(x=>
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
        
        await _context.Requests.AddAsync(newRequest);
        
        
        await _context.SaveChangesAsync();
        await UpdateTotalsIntoCart(newRequest.CartId.Value);
        await ApplyDiscountToCart(newRequest.CartId.Value);

        return newRequest;
    }

}