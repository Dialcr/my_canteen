namespace Canteen.Services.Dto;

public class EstablishmentOutputDto
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Image { get; set; }

    public string Description { get; set; }

    public ICollection<Order>? Orders { get; set; }

    public ICollection<Product>? Products { get; set; }

    public ICollection<Menu>? Menus { get; set; }

    public ICollection<Discount>? Discounts { get; set; }

    public ICollection<DeliveryTime>? DeliveryTimes { get; set; }
}

public static class EstablishmentExtention
{
    public static EstablishmentOutputDto ToEstablishmentOutputDto(this Establishment establishment)
    {
        
        return new EstablishmentOutputDto()
        {
            Description = establishment.Description,
            Id = establishment.Id,
            Image = establishment.Image,
            Name = establishment.Name,
            
        };

    }
}