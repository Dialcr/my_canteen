using Canteen.DataAccess;
using Canteen.DataAccess.Enums;
using Canteen.Services.Abstractions;
using Canteen.Services.Dto;
using Canteen.Services.Dto.CanteenRequest;
using Canteen.Services.Dto.Order;

namespace Canteen.Services.Services;

public class CanteenOrderServices(EntitiesContext context, MenuServices menuServices) : CustomServiceBase(context), ICanteenOrderServices
{
    public async Task<OneOf<ResponseErrorDto, Order>> ApplyDiscountToOrderAsync(
        int orderId)
    {
        var order = await context.Orders.FindAsync(orderId);
        if (order is null)
        {
            const string errorMessage = "Order not found";
            const string errorDetail = $"The order with has not found";
            const int errorStatus = 400;

            return Error(errorMessage, errorDetail, errorStatus);
        }

        var totalDiscount = GetApplicableDiscount(order.EstablishmentId, order.PrductsTotalAmount, DiscountType.TotalAmount);
        var deliveryDiscount = GetApplicableDiscount(order.EstablishmentId, order.DeliveryTotalAmount, DiscountType.DeliveryAmount);

        const decimal defaultDiscountValue = 1m;
        order.ProductTotalDiscount = totalDiscount?.DiscountDecimal ?? defaultDiscountValue;
        order.DeliveryTotalDiscount = deliveryDiscount?.DiscountDecimal ?? defaultDiscountValue;

        await context.SaveChangesAsync();

        return order;
    }

    private Discount? GetApplicableDiscount(int establishmentId, decimal amount, DiscountType discountType)
    {
        return context.Discounts
            .Where(x => x.Establishment!.Id == establishmentId
                        && x.DiscountType.Equals(discountType)
                        && amount <= x.TotalNecesity)
            .OrderByDescending(x => x.TotalNecesity)
            .FirstOrDefault();
    }


    public async Task<OneOf<ResponseErrorDto, Order>> UpdateTotalsAsync(int orderId)
    {
        var order = await context.Orders.Include(x => x.Requests).FirstOrDefaultAsync(x => x.Id == orderId);
        if (order is null)
        {
            return Error("Order not found",
                $"The order with id {orderId} has not found",
                400);
        }
        order.PrductsTotalAmount = order.Requests!.Where(x => x.Status == RequestStatus.Planned)
            .Sum(x => x.TotalAmount);
        order.DeliveryTotalAmount = order.Requests!.Where(x => x.Status == RequestStatus.Planned)
            .Sum(x => x.DeliveryAmount);
        await context.SaveChangesAsync();
        return order;

    }

    public async Task<OneOf<ResponseErrorDto, Order>> CloseOrderIfAllRequestsClosedAsync(int orderId)
    {
        var order = await context.Orders.Include(o => o.Requests)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.Status == OrderStatus.Created);

        if (order == null)
        {
            return Error("Order not found",
                $"The order with id {orderId} has not found",
                400);
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
        await context.SaveChangesAsync();

        return order;
    }

    public async Task<OneOf<ResponseErrorDto, Order>> CancelOrderAsync(int orderId)
    {
        var order = await context.Orders.Include(x => x.Requests)!.
            ThenInclude(request => request.RequestProducts!)
            .FirstOrDefaultAsync(x => x.Id == orderId && x.Status == OrderStatus.Created);

        if (order is null)
        {
            return Error("Order not found",
                $"The order with id {orderId} has not found",
                400);

        }

        foreach (var orderRequest in order.Requests!)
        {

            orderRequest.Status = RequestStatus.Cancelled;

            Menu originDayMenu = context.Menus
                .Include(x => x.MenuProducts)
                .SingleOrDefault(x => x.Date.Date == orderRequest.DeliveryDate.Date! && x.EstablishmentId == orderRequest.Order!.EstablishmentId)!;
            foreach (var dayProduct in originDayMenu.MenuProducts!)
            {

                var requestproduct = orderRequest.RequestProducts!.FirstOrDefault(x =>
                    x.ProductId == dayProduct.CanteenProductId);
                if (requestproduct is not null)
                {
                    dayProduct.Quantity += requestproduct.Quantity;
                }
            };

            return Error("Request not found",
                $"Request with id {orderRequest.Id} and status {RequestStatus.Planned} not found",
                400);
        }
        await UpdateTotalsAsync(orderId);
        await ApplyDiscountToOrderAsync(orderId);
        await CloseOrderIfAllRequestsClosedAsync(orderId);

        order.Status = OrderStatus.Cancelled;
        await context.SaveChangesAsync();
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
            PrductsTotalAmount = cart.Requests.Sum(x => x.TotalAmount),
            DeliveryTotalAmount = cart.Requests.Sum(x => x.DeliveryAmount),
            UserId = cart.UserId

        };
        context.Orders.Add(newOrder);
        await context.SaveChangesAsync();
        return newOrder;

    }



    public async Task<OneOf<ResponseErrorDto, CanteenRequest>> EditRequestIntoOrderAsync(
        EditRequestDto requestDto)
    {

        var request = await context.Requests
            .Include(r => r.RequestProducts)!.ThenInclude(requestProduct => requestProduct.Product)
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
        var requestProducts = new List<RequestProduct>();
        foreach (var product in requestDto.Products)
        {
            var existingProduct = request.RequestProducts.FirstOrDefault(p => p.ProductId == product.ProductId);
            var aviableProduct = context.MenuProducts.Include(x => x.Product)
                .FirstOrDefault(x => x.CanteenProductId == product.ProductId && x.Menu!.Date.Date == request.DeliveryDate.Date);
            if (aviableProduct is null)
            {
                return Error("Product not found",
                    $"The product with id {product.ProductId} has not been found",
                    400);
            }
            if (existingProduct is not null)
            {
                if (product.Quantity < existingProduct.Quantity && product.Quantity != 0)
                {
                    existingProduct.Quantity = product.Quantity;
                    aviableProduct.Quantity += existingProduct.Quantity - product.Quantity;
                }
                else if (product.Quantity > existingProduct.Quantity && product.Quantity - existingProduct.Quantity <= aviableProduct.Quantity
                                                                     && product.Quantity != 0)
                {
                    existingProduct.Quantity = product.Quantity;
                    aviableProduct.Quantity -= product.Quantity - existingProduct.Quantity;
                }
                else if (existingProduct.Quantity != product.Quantity)
                {
                    return Error("Insufficient stock",
                        $"The product with id {product.ProductId} does not have sufficient stock",
                        400);
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
                    return Error("Insufficient stock",
                        $"The product with id {product.ProductId} does not have sufficient stock",
                        400);
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
        request.TotalAmount = request.RequestProducts.Sum(x => x.UnitPrice * x.Quantity);
        await context.SaveChangesAsync();

        return request;
    }
    public async Task<OneOf<ResponseErrorDto, CanteenRequest>> PlanningRequestIntoOrderAsync(
        int requestId,
        int establishmentId,
        DateTime newDateTime)
    {
        var request = await context.Requests
            .Include(x => x.Order)
            .Include(request => request.RequestProducts!)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request is null)
        {
            return Error("Request not found",
                $"The request with id {requestId} was not found",
                400);
        }

        if (!request.Status.Equals(RequestStatus.Planned))
        {
            return Error("Invalid request status",
                "The request status has changed and cannot be planned",
                400);
        }

        var dayMenuResult = menuServices.GetMenuByEstablishmentAndDate(establishmentId, newDateTime);

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
            var aviableProduct = dayMenu.MenuProducts!.FirstOrDefault(x => x.Product!.Id == requestProduct.ProductId && x.Quantity >= requestProduct.Quantity);
            if (aviableProduct is null)
            {
                return Error("Insufficient stock",
                    $"The product with id {requestProduct.ProductId} does not have sufficient stock",
                    400);
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
        context.Requests.Add(newCanteenRequest);
        await context.SaveChangesAsync();
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
        var request = context.Requests.Include(x => x.Order)
            .Include(x => x.RequestProducts)
            .Include(x => x.Order)
            .SingleOrDefault(x =>
                x.Id == requestId &&
                x.Status.Equals(RequestStatus.Planned)
                && (x.OrderId != null));

        if (request is not null)
        {
            request.Status = RequestStatus.Cancelled;

            var originDayMenu = context.Menus
                .Include(x => x.MenuProducts)
                .SingleOrDefault(x => x.Date.Date == request.DeliveryDate.Date! && x.EstablishmentId == request.Order!.EstablishmentId)!;
            foreach (var dayProduct in originDayMenu.MenuProducts!)
            {
                var requestproduct = request.RequestProducts!.FirstOrDefault(x =>
                    x.ProductId == dayProduct.CanteenProductId);
                if (requestproduct is not null)
                {
                    dayProduct.Quantity += requestproduct.Quantity;
                }
            };

            await UpdateTotalsAsync(request.OrderId!.Value);
            await ApplyDiscountToOrderAsync(request.OrderId!.Value);
            await CloseOrderIfAllRequestsClosedAsync(request.OrderId.Value);

            await context.SaveChangesAsync();
            return request.ToCanteenRequestOutputDto();
        }

        return Error("Request not found",
            $"Request with id {requestId} and status {RequestStatus.Planned} not found",
            400);
    }

    public async Task<OneOf<ResponseErrorDto, OrderOutputDto>> GetOrderByUserIdAsync(int userId)
    {
        var order = await context.Orders
            .Include(x => x.Requests)
            .ThenInclude(x => x.RequestProducts)
            .ThenInclude(x => x.Product)
            .FirstOrDefaultAsync(x => x.UserId == userId);
        if (order is null)
        {
            return Error("Cart not found",
                $"The cart of user with id {userId} has not found",
                400);
        }
        return order.ToOrderOutputDto();
    }
}