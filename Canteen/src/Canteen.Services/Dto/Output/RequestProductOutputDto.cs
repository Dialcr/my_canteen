using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Canteen.DataAccess.Enums;

namespace Canteen.Services.Dto;

public class RequestProductOutputDto
{
    public int Id { get; set; }
    
    public int RequestId { get; set; }
    
    public int ProductId { get; set; }
    
    public string ProuctName { get; set; }

    public string ProuctDescription { get; set; }

    public ProductCategory Category { get; set; }

   public decimal ProductPrice { get; set; }

    public int EstablishmentId { get; set; }

}