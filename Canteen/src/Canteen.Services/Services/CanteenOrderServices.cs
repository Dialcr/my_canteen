using Canteen.DataAccess;
using Canteen.DataAccess.Enums;
using Canteen.Services.Dto;

namespace Canteen.Services.Services;

public class CanteenOrderServices : CustomServiceBase
{
    

    public CanteenOrderServices(EntitiesContext context)
        : base(context)
    {
     
    }

    public async Task<OneOf<ResponseErrorDto, Order>> ApplyDiscountToOrder(
        int orderId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order is null)
        {
            return new ResponseErrorDto()
            {
                Status = 404,
                Title = "Order not found",
                Detail = $"The order with id {orderId} has not found"
            };
        }

        var totalDiscount = _context.Discounts.Where(x => x.Establishment.Id == order.EstablishmentId
                                                          && x.DiscountType.Equals(DiscountType.TotalAmount.ToString())
                                                          && order.PrductsTotalAmount <= x.TotalNecesity)
            .OrderByDescending(x => x.TotalNecesity)
            .FirstOrDefault();

        var delivaryDiscount = _context.Discounts.Where(x => x.Establishment.Id == order.EstablishmentId
                                                             && x.DiscountType.Equals(DiscountType.DeliveryAmount
                                                                 .ToString())
                                                             && order.DeliveryTotalAmount <= x.TotalNecesity)
            .OrderByDescending(x => x.TotalNecesity)
            .FirstOrDefault();


        order.ProductTotalDiscount = (totalDiscount is not null) ? totalDiscount.DiscountDecimal : 1;
        order.DeliveryTotalDiscount = (delivaryDiscount is not null) ? delivaryDiscount.DiscountDecimal : 1;

        if (order.ProductTotalDiscount == 1)
        {
            return new ResponseErrorDto()
            {
                Status = 404,
                Title = "Discount not found",
                Detail = $"Not total discount available"
            };
        }

        if (order.DeliveryTotalDiscount == 1)
        {
            return new ResponseErrorDto()
            {
                Status = 404,
                Title = "Discount not found",
                Detail = $"Not delivery discount available"
            };
        }

        await _context.SaveChangesAsync();

        return order;
    }

    public async Task<OneOf<ResponseErrorDto, Order>> UpdateTotals(int orderId)
    {
        var order = await _context.Orders.Include(x => x.Requests).FirstOrDefaultAsync(x => x.Id == orderId);
        if (order is null)
        {
            return new ResponseErrorDto()
            {
                Status = 404,
                Title = "Order not found",
                Detail = $"The order with id {orderId} has not found"
            };
        }
        order.PrductsTotalAmount = order.Requests!.Where(x => x.Status == RequestStatus.Planned.ToString())
            .Sum(x => x.TotalAmount);
        order.DeliveryTotalAmount = order.Requests!.Where(x => x.Status == RequestStatus.Planned.ToString())
            .Sum(x => x.DeliveryAmount);
        await _context.SaveChangesAsync();
        return order;

    }

    public async Task<OneOf<ResponseErrorDto, Order>> CloseOrderIfAllRequestsClosed(int orderId)
    {
        var order = await _context.Orders.Include(o => o.Requests)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.Status == OrderStatus.Created.ToString());

        if (order == null)
        {
            return new ResponseErrorDto()
            {
                Status  = 404,
                Title = "Order not found",
                Detail = $"The order with id {orderId} has not found with status {OrderStatus.Created}"
            };
        }
        //si todas estan en estado cancelada la orden se cancela
        if (order.Requests!.All(r => r.Status == RequestStatus.Cancelled.ToString()))
        {
            order.Status = OrderStatus.Cancelled.ToString();
        }
        // si todas estan entregadas o canceladas la orden se cierra
        else if (order.Requests!.All(r => r.Status == RequestStatus.Delivered.ToString() 
                                          || r.Status == RequestStatus.Cancelled.ToString()))
        {
            order.Status = OrderStatus.Close.ToString();
        }
        await _context.SaveChangesAsync();

        return order;
    }

    public async Task<OneOf<ResponseErrorDto, Order>> CancelOrder(int orderId)
    {
        var order = await _context.Orders.Include(x => x.Requests)
            .FirstOrDefaultAsync(x => x.Id == orderId && x.Status == OrderStatus.Created.ToString());

        if (order is null)
        {
            return new ResponseErrorDto()
            {
                Status = 404,
                Title = "Order not found",
                Detail = $"The order with id {orderId} has not found with status {OrderStatus.Created}"
            };

        }
        foreach (var orderRequest in order.Requests!)
        {
            orderRequest.Status = RequestStatus.Cancelled.ToString();

            Menu originDayMenu = _context.Menus
                .Include(x=>x.MenuProducts)
                .SingleOrDefault(x => x.Date == orderRequest.DeliveryDate! && x.EstablishmentId == orderRequest.Order!.EstablishmentId)!;
            foreach (var dayProduct in originDayMenu.MenuProducts!)
            {
            
                //if (response.RequestProducts.Contains(dayProduct.CanteenProductId)) dayProduct.Quantity--;
                var requestproduct = orderRequest.RequestProducts!.FirstOrDefault(x =>
                    x.ProductId == dayProduct.CanteenProductId);
                if ( requestproduct is not null)
                {
                    dayProduct.Quantity += requestproduct.Quantity;
                }
            };
            await _context.SaveChangesAsync();
            return new ResponseErrorDto()
            {
                Status = 404,
                Title = "Request not found",
                Detail = $"Request with id {orderRequest.Id} and status {RequestStatus.Planned} not found"
            };
        }
        await UpdateTotals(orderId);
        await ApplyDiscountToOrder(orderId);
        await CloseOrderIfAllRequestsClosed(orderId);
    
        order.Status = OrderStatus.Cancelled.ToString();
        await _context.SaveChangesAsync();
        return order;
    }

//este nmetodo creo que no hay qy=ue tenerlo 
/*
 *
public OneOf<ResponseErrorDto, Order> CreateOrder(List<Request> requests, int userId, int establishmentId)
{
    foreach (var request in requests)
    {
        if (request.OrderId is not null ||request.OrderId!=0)
        {

        }
    }
    var newOrder = new Order
    {
        CreatedAt = DateTime.Now,
        Status = OrderStatus.Created.ToString(),
        Requests = requests.ToList(),
        EstablishmentId = establishmentId,
        PrductsTotalAmount = requests.Sum(x=>x.TotalAmount),
        DeliveryTotalAmount = requests.Sum(x=>x.DeliveryAmount),
        //ProductTotalDiscount = 0,
        //DeliveryTotalDiscount = 0

    };
    _context.Orders.Add(newOrder);
    _context.SaveChanges();
    return newOrder;

}

 */
}