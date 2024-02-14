using Canteen.DataAccess;
using Canteen.DataAccess.Enums;
using Canteen.Services.Dto;

namespace Canteen.Services.Services;

public class CanteenOrderServices : CustomServiceBase
{
    private readonly RequestServices _requestServices;
    public CanteenOrderServices(EntitiesContext context, RequestServices requestServices)
        : base(context)
    {
        _requestServices = requestServices;
    }

    public async Task ApplyDiscountToOrder(
        int orderId,
        decimal discountAmount)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null)
        {
            throw new ArgumentException("Order not found");
        }
        //todo: como se va a aplicar el decuento   
        order.ProductTotalDiscount = discountAmount;
        await _context.SaveChangesAsync();
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
        //todo: cancelada es un estado necesario de la orden?
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
            var result = await _requestServices.CancelRequest(orderRequest.Id);
            if (result.TryPickT0(out var error, out var response))
            {
                return new ResponseErrorDto()
                {
                    Status = 400,
                    Title = "Cancel order not success",
                    Detail = $"The cancel order with id {orderId} not success, request with id {orderRequest.Id} not found with status {RequestStatus.Planned}"
                    
                };
                
            }
        }
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
            //todo: hay que sacar el descuento
            //ProductTotalDiscount = 0,
            //DeliveryTotalDiscount = 0

        };
        _context.Orders.Add(newOrder);
        _context.SaveChanges();
        return newOrder;

    }

     */
}