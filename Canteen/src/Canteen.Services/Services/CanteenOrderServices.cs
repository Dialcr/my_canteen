using Canteen.DataAccess;
using Canteen.DataAccess.Enums;
using Canteen.Services.Dto;

namespace Canteen.Services.Services;

public class CanteenOrderServices : CustomServiceBase
{
    private readonly MenuServices _menuServices;

    public CanteenOrderServices(EntitiesContext context, MenuServices menuServices)
        : base(context)
    {
        _menuServices = menuServices;
    }

    public async Task<OneOf<ResponseErrorDto, Order>> ApplyDiscountToOrderAsync(
        int orderId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order is null)
        {
            return new ResponseErrorDto()
            {
                Status = 400,
                Title = "Order not found",
                Detail = $"The order with id {orderId} has not found"
            };
        }

        var totalDiscount = _context.Discounts.Where(x => x.Establishment!.Id == order.EstablishmentId
                                                          && x.DiscountType.Equals(DiscountType.TotalAmount)
                                                          && order.PrductsTotalAmount <= x.TotalNecesity)
            .OrderByDescending(x => x.TotalNecesity)
            .FirstOrDefault();

        var delivaryDiscount = _context.Discounts.Where(x => x.Establishment!.Id == order.EstablishmentId
                                                             && x.DiscountType.Equals(DiscountType.DeliveryAmount)
                                                             && order.DeliveryTotalAmount <= x.TotalNecesity)
            .OrderByDescending(x => x.TotalNecesity)
            .FirstOrDefault();


        order.ProductTotalDiscount = (totalDiscount is not null) ? totalDiscount.DiscountDecimal : 1;
        order.DeliveryTotalDiscount = (delivaryDiscount is not null) ? delivaryDiscount.DiscountDecimal : 1;
        

        await _context.SaveChangesAsync();

        return order;
    }

    public async Task<OneOf<ResponseErrorDto, Order>> UpdateTotalsAsync(int orderId)
    {
        var order = await _context.Orders.Include(x => x.Requests).FirstOrDefaultAsync(x => x.Id == orderId);
        if (order is null)
        {
            return new ResponseErrorDto()
            {
                Status = 400,
                Title = "Order not found",
                Detail = $"The order with id {orderId} has not found"
            };
        }
        order.PrductsTotalAmount = order.Requests!.Where(x => x.Status == RequestStatus.Planned)
            .Sum(x => x.TotalAmount);
        order.DeliveryTotalAmount = order.Requests!.Where(x => x.Status == RequestStatus.Planned)
            .Sum(x => x.DeliveryAmount);
        await _context.SaveChangesAsync();
        return order;

    }

    public async Task<OneOf<ResponseErrorDto, Order>> CloseOrderIfAllRequestsClosedAsync(int orderId)
    {
        var order = await _context.Orders.Include(o => o.Requests)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.Status == OrderStatus.Created);

        if (order == null)
        {
            return new ResponseErrorDto()
            {
                Status  = 400,
                Title = "Order not found",
                Detail = $"The order with id {orderId} has not found with status {OrderStatus.Created}"
            };
        }
        if (order.Requests!.All(r => r.Status == RequestStatus.Cancelled))
        {
            order.Status = OrderStatus.Cancelled;
        }
        else if (order.Requests!.All(r => r.Status == RequestStatus.Delivered 
                                          || r.Status == RequestStatus.Cancelled))
        {
            order.Status = OrderStatus.Close;
        }
        await _context.SaveChangesAsync();

        return order;
    }

    public async Task<OneOf<ResponseErrorDto, Order>> CancelOrderAsync(int orderId)
    {
        var order = await _context.Orders.Include(x => x.Requests)!.
            ThenInclude(request => request.RequestProducts!)
            .FirstOrDefaultAsync(x => x.Id == orderId && x.Status == OrderStatus.Created);

        if (order is null)
        {
            return new ResponseErrorDto()
            {
                Status = 400,
                Title = "Order not found",
                Detail = $"The order with id {orderId} has not found with status {OrderStatus.Created}"
            };

        }
        
        foreach (var orderRequest in order.Requests!)
        {
            
            orderRequest.Status = RequestStatus.Cancelled;

            Menu originDayMenu = _context.Menus
                .Include(x=>x.MenuProducts)
                .SingleOrDefault(x => x.Date.Date == orderRequest.DeliveryDate.Date! && x.EstablishmentId == orderRequest.Order!.EstablishmentId)!;
            foreach (var dayProduct in originDayMenu.MenuProducts!)
            {
            
                var requestproduct = orderRequest.RequestProducts!.FirstOrDefault(x =>
                    x.ProductId == dayProduct.CanteenProductId);
                if ( requestproduct is not null)
                {
                    dayProduct.Quantity += requestproduct.Quantity;
                }
            };
           
            return new ResponseErrorDto()
            {
                Status = 400,
                Title = "Request not found",
                Detail = $"Request with id {orderRequest.Id} and status {RequestStatus.Planned} not found"
            };
        }
        await UpdateTotalsAsync(orderId);
        await ApplyDiscountToOrderAsync(orderId);
        await CloseOrderIfAllRequestsClosedAsync(orderId);
    
        order.Status = OrderStatus.Cancelled;
        await _context.SaveChangesAsync();
        return order;
    }
    
    public async Task<Order> CreateOrderAsync(CanteenCart cart)
    {
        var newOrder = new Order
        {
            CreatedAt = DateTime.Now,
            Status = OrderStatus.Created,
            Requests = cart.Requests,
            EstablishmentId = cart.EstablishmentId,
            PrductsTotalAmount =  cart.Requests.Sum(x=>x.TotalAmount),
            DeliveryTotalAmount = cart.Requests.Sum(x=>x.DeliveryAmount),
            UserId = cart.UserId

        };
        _context.Orders.Add(newOrder);
        await _context.SaveChangesAsync();
        return newOrder;

    }

    

    public async Task<OneOf<ResponseErrorDto, CanteenRequest>> EditRequestIntoOrderAsync(
        EditRequestDto requestDto)
    {
       
        var request = await _context.Requests
            .Include(r => r.RequestProducts)!.ThenInclude(requestProduct => requestProduct.Product)
            .FirstOrDefaultAsync(r => r.Id == requestDto.RequestId);

        if (request is null)
        {
            return new ResponseErrorDto
            {
                Status = 400,
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
        var requestProducts = new List<RequestProduct>();
        foreach (var product in requestDto.Products)
        {
            var existingProduct = request.RequestProducts.FirstOrDefault(p => p.ProductId == product.ProductId);
            var aviableProduct = _context.MenuProducts.Include(x => x.Product)
                .FirstOrDefault(x=>x.CanteenProductId == product.ProductId && x.Menu!.Date.Date == request.DeliveryDate.Date);
            if (aviableProduct is null)
            {
                return new ResponseErrorDto()
                {
                    Status = 400,
                    Title = "Product not found",
                    Detail = $"The product with id {product.ProductId} has not been found"
                };
            }
            if (existingProduct is not null)
            {
                if (product.Quantity < existingProduct.Quantity && product.Quantity!=0)
                {
                    existingProduct.Quantity = product.Quantity;
                    aviableProduct.Quantity += existingProduct.Quantity - product.Quantity;
                }
                else if (product.Quantity > existingProduct.Quantity && product.Quantity- existingProduct.Quantity<=aviableProduct.Quantity 
                                                                     && product.Quantity!=0)
                {
                    existingProduct.Quantity = product.Quantity;
                    aviableProduct.Quantity -= product.Quantity-existingProduct.Quantity ;
                }
                else if (existingProduct.Quantity !=product.Quantity)
                {
                    return new ResponseErrorDto
                    {
                        Status = 400,
                        Title = "Insufficient stock",
                        Detail = $"The product with id {product.ProductId} does not have sufficient stock"
                    };
                }
                requestProducts.Add(new RequestProduct()
                {
                    ProductId = product.ProductId,
                    RequestId = request.Id,
                    Quantity = product.Quantity,
                    UnitPrice = aviableProduct.Product!.Price
                });
                
            }
            else
            {
                if (product.Quantity > aviableProduct.Quantity)
                {
                    return new ResponseErrorDto
                    {
                        Status = 400,
                        Title = "Insufficient stock",
                        Detail = $"The product with id {product.ProductId} does not have sufficient stock"
                    };
                }
                aviableProduct.Quantity -= product.Quantity;
                requestProducts.Add(new RequestProduct()
                {
                    ProductId = product.ProductId,
                    RequestId = request.Id,
                    Quantity = product.Quantity,
                    UnitPrice = aviableProduct.Product!.Price
                });
            }
        }
        request.RequestProducts = requestProducts;
        request.DeliveryDate = requestDto.DeliveryDate;
        request.DeliveryLocation = requestDto.DeliveryLocation;
        request.DeliveryAmount = requestDto.DeliveryAmount;
        request.DeliveryTimeId = requestDto.DeliveryTimeId;
        await UpdateTotalsAsync(request.OrderId!.Value);
        await ApplyDiscountToOrderAsync(request.OrderId!.Value);
        request.TotalAmount = request.RequestProducts.Sum(x=>x.UnitPrice * x.Quantity);
        await _context.SaveChangesAsync();

        return request;
    }
    public async Task<OneOf<ResponseErrorDto, CanteenRequest>> PlanningRequestIntoOrderAsync(
        int requestId,
        int establishmentId,
        DateTime newDateTime)
    {
        var request = await _context.Requests
            .Include(x => x.Order)
            .Include(request => request.RequestProducts!)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request is null)
        {
            return new ResponseErrorDto
            {
                Status = 400,
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
                Detail = "The request status has changed and cannot be planned"
            };
        }

        var dayMenuResult = _menuServices.GetMenuByEstablishmentAndDate(establishmentId, newDateTime);

        if (dayMenuResult.TryPickT0(out var error, out var dayMenu))
        {
            return error;
        }
        var newCanteenRequest = new CanteenRequest()
        {
            DeliveryDate = newDateTime,
            DeliveryLocation = request.DeliveryLocation,
            Status = RequestStatus.Planned,
            OrderId = request.OrderId,
            UserId = request.UserId,
            CreatedAt = DateTimeOffset.Now,
            TotalAmount = request.TotalAmount,
            RequestProducts = new List<RequestProduct>(),
            DeliveryTimeId = request.DeliveryTimeId,
            DeliveryAmount = request.DeliveryAmount,
            
        };
        foreach (var requestProduct in request.RequestProducts!)
        {
            var aviableProduct = dayMenu.MenuProducts!.FirstOrDefault(x => x.Product!.Id == requestProduct.ProductId && x.Quantity >= requestProduct.Quantity );
            if (aviableProduct is null)
            {
                return new ResponseErrorDto()
                {
                    Status  = 400,
                    Title = "Insufficient stock",
                    Detail = $"The product with id {requestProduct.ProductId} does not have sufficient stock"
                };
            }
            else
            {
                newCanteenRequest.RequestProducts.Add(new RequestProduct()
                {
                    ProductId = requestProduct.ProductId,
                    RequestId = request.Id,
                    Quantity = requestProduct.Quantity,
                    
                    
                });
                aviableProduct.Quantity -= requestProduct.Quantity;
            }
        }
        _context.Requests.Add(newCanteenRequest);
        await _context.SaveChangesAsync();
        await UpdateTotalsAsync(request.OrderId!.Value);
        var discount = await ApplyDiscountToOrderAsync(request.OrderId!.Value);
        if (discount.TryPickT0(out var discountError, out _))
        {
            return discountError;
        }
        return newCanteenRequest;

    }
    public async Task<OneOf<ResponseErrorDto, RequestOutputDto>> CancelRequestIntoOrderAsync(int requestId)
    {
        var request = _context.Requests.Include(x => x.Order)
            .Include(x => x.RequestProducts)
            .Include(x => x.Order)
            .SingleOrDefault(x =>
                x.Id == requestId &&
                x.Status.Equals(RequestStatus.Planned)
                && (x.OrderId != null));

        if (request is not null)
        {
            request.Status = RequestStatus.Cancelled;

            var originDayMenu = _context.Menus
                .Include(x=>x.MenuProducts)
                .SingleOrDefault(x => x.Date.Date == request.DeliveryDate.Date! && x.EstablishmentId == request.Order!.EstablishmentId)!;
            foreach (var dayProduct in originDayMenu.MenuProducts!)
            {
                var requestproduct = request.RequestProducts!.FirstOrDefault(x =>
                    x.ProductId == dayProduct.CanteenProductId);
                if ( requestproduct is not null)
                {
                    dayProduct.Quantity += requestproduct.Quantity;
                }
            };
            
            await UpdateTotalsAsync(request.OrderId!.Value);
            await ApplyDiscountToOrderAsync(request.OrderId!.Value);
            await CloseOrderIfAllRequestsClosedAsync(request.OrderId.Value);
            
            await _context.SaveChangesAsync();
            return request.ToCanteenRequestOutputDto();
        }

        return new ResponseErrorDto()
        {
            Status = 400,
            Title = "Request not found",
            Detail = $"Request with id {requestId} and status {RequestStatus.Planned} not found"
        };
    }

    public async Task<OneOf<ResponseErrorDto, OrderOutputDto>> GetOrderByUserIdAsync(int userId)
    {
        var order = await _context.Orders
            .Include(x => x.Requests)
            .ThenInclude(x=>x.RequestProducts)
            .ThenInclude(x=>x.Product)
            .FirstOrDefaultAsync(x => x.UserId == userId);
        if (order is null)
        {
            return new ResponseErrorDto()
            {
                Status = 400,
                Title = "Cart not found",
                Detail = $"The cart of user with id {userId} has not found"
            };
        }
        return order.ToOrderOutputDto();
    }
}