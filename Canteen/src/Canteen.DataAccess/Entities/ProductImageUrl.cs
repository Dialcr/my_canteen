namespace Canteen.DataAccess.Entities;

public class ProductImageUrl
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(500)]
    public string Url { get; set; } = "";

    public int ProductId { get; set; }

    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; } = null!;


}