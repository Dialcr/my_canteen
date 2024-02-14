namespace Canteen.DataAccess.Entities;


/// <summary>
/// Table used to store key/value pairs
/// </summary>
public class KeyValueData
{
    [Key]
    [MaxLength(100)]
    public string Key { get; set; } = "";

    [Required]
    // no MaxLength here
    public string Data { get; set; } = "";
}