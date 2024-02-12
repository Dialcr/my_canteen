using Canteen.DataAccess;

namespace Canteen.Services;

public class CanteenOrderServices : CustomServiceBase
{
    public CanteenOrderServices(EntitiesContext context)
        : base(context)
    {
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

        order.ProductTotalDiscount = discountAmount;
        await _context.SaveChangesAsync();
    }

    public async Task CloseOrderIfAllRequestsClosed(int orderId)
    {
        var order = await _context.Orders.Include(o => o.Requests).FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            throw new ArgumentException("Order not found");
        }

        if (order.Requests.All(r => r.Status != "Open"))
        {
            order.Status = "Closed";
            await _context.SaveChangesAsync();
        }
    }
}