
namespace Canteen.DataAccess.Settings;

public class JwtSettings
{
    [Required]
    public string Key { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Range(1, double.MaxValue)]
    [Required]
    public double LifeTimeToken { get; set; }
}
