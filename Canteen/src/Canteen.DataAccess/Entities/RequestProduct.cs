namespace Canteen.DataAccess.Entities;

public class RequestProduct
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int RequestId { get; set; }
    
    [ForeignKey(nameof(RequestId))]
    public CanteenRequest CanteenRequest { get; set; }
    
    [Required]
    public int ProductId { get; set; }
    
    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; }

    [Required] public int Quantity { get; set; } = 0;
    
    public decimal UnitPrice { get; set; }
}