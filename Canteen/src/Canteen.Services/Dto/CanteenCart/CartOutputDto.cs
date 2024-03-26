namespace Canteen.Services.Dto;

public class CartOutputDto
{
    
    public int Id { get; set; }

    public int EstablishmentId { get; set; }
    public EstablishmentOutputDto? Establishment { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public decimal PrductsTotalAmount { get; set; }
    public decimal ProductTotalDiscount { get; set; }
    public decimal DeliveryTotalAmount { get; set; }
    public decimal DeliveryTotalDiscount { get; set; }

    public ICollection<RequestOutputDto>? Requests { get; set; }
    public int UserId { get; set; }
}

public static class CartingCartExtention
{
    public static CartOutputDto ToCanteenCartDto(this CanteenCart cart)
    {
        return new CartOutputDto()
        {
            Id = cart.Id,
            EstablishmentId = cart.EstablishmentId,
            CreatedAt = cart.CreatedAt,
            PrductsTotalAmount = cart.PrductsTotalAmount,
            ProductTotalDiscount = cart.ProductTotalDiscount,
            DeliveryTotalAmount = cart.DeliveryTotalAmount,
            DeliveryTotalDiscount = cart.DeliveryTotalDiscount,
            UserId = cart.UserId,
            Requests = cart.Requests!.Select(x => x.ToCanteenRequestWithProductsDto()).ToList()
        };

    }
}