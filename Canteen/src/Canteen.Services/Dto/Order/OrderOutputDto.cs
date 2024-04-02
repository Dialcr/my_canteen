using Canteen.DataAccess.Enums;
using Canteen.Services.Dto.CanteenRequest;

namespace Canteen.Services.Dto.Order;

public class OrderOutputDto
{
    public int Id { get; set; }
    public int EstablishmentId { get; set; }

    public OrderStatus Status { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? CanceledAt { get; set; }

    public decimal PrductsTotalAmount { get; set; }
    public decimal ProductTotalDiscount { get; set; }
    public decimal DeliveryTotalAmount { get; set; }
    public decimal DeliveryTotalDiscount { get; set; }

    public ICollection<RequestOutputDto>? Requests { get; set; }
    public int UserId { get; set; }
}

public static class OrderExtention
{
    public static OrderOutputDto ToOrderOutputDto(this DataAccess.Entities.Order order)
    {
        
        return new OrderOutputDto()
        {
            Id = order.Id,
            CreatedAt = order.CreatedAt,
            EstablishmentId = order.EstablishmentId,
            CanceledAt = order.CanceledAt,
            DeliveryTotalAmount = order.DeliveryTotalAmount,
            DeliveryTotalDiscount = order.DeliveryTotalDiscount,
            PrductsTotalAmount = order.PrductsTotalAmount,
            ProductTotalDiscount = order.ProductTotalDiscount,
            Status = order.Status,
            UpdatedAt = order.UpdatedAt,
            UserId = order.UserId,
            Requests = order.Requests!.Select(x => x.ToCanteenRequestWithProductsDto()).ToList()
        };
    }
}