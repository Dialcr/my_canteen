using Canteen.DataAccess.Enums;

namespace Canteen.Services.Dto.CanteenRequest;

public class RequestOutputDto
{
    public int Id { get; set; }
    
    public int? OrderId { get; set; }
    public int UserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public DateTimeOffset DeliveryDate { get; set; }

    public string DeliveryLocation { get; set; }

    public decimal TotalAmount { get; set; }
    public decimal DeliveryAmount { get; set; }

    public IEnumerable<RequestProductDto>? RequestProducts { get; set; }

    public RequestStatus Status { get; set; }
    
    public int? CartId { get; set; }

    public int DeliveryTimeId { get; set; }
    public DeliveryTime? DeliveryTime { get; set; }
}


public static class RequestExtention
{
    public static RequestOutputDto ToCanteenRequestWithProductsDto(this DataAccess.Entities.CanteenRequest request)
    {
        
        return new RequestOutputDto()
        {
            CreatedAt = request.CreatedAt,
            DeliveryDate = request.DeliveryDate,
            DeliveryLocation = request.DeliveryLocation,
            Id = request.Id,
            OrderId = request.OrderId,
            Status = request.Status,
            UserId = request.UserId,
            DeliveryAmount = request.DeliveryAmount,
            DeliveryTimeId = request.DeliveryTimeId,
            TotalAmount = request.TotalAmount,
            CartId = request.CartId,
            UpdatedAt = request.UpdatedAt,
            DeliveryTime = request.DeliveryTime ,
            RequestProducts = request.RequestProducts!.Select(x => new RequestProductDto()
            {
                Quantity = x.Quantity,
                ProductId = x.ProductId,
                UnitPrice = x.UnitPrice,
                ProductName = x.Product.Name
            })
        };

    }public static RequestOutputDto ToCanteenRequestOutputDto(this DataAccess.Entities.CanteenRequest request)
    {
        
        return new RequestOutputDto()
        {
            CreatedAt = request.CreatedAt,
            DeliveryDate = request.DeliveryDate,
            DeliveryLocation = request.DeliveryLocation,
            Id = request.Id,
            OrderId = request.OrderId,
            Status = request.Status,
            UserId = request.UserId,
            DeliveryAmount = request.DeliveryAmount,
            DeliveryTimeId = request.DeliveryTimeId,
            TotalAmount = request.TotalAmount,
            CartId = request.CartId,
            UpdatedAt = request.UpdatedAt,
            DeliveryTime = request.DeliveryTime ,
            RequestProducts = null
        };

    }
}