
namespace Canteen.Services.Dto.User;

public class UserOutputDto
{
    public string Name { get; set; } = "";

    public string Email { get; set; } = "";

    public IEnumerable<string> Role { get; set; } = [];
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;


}

public static class UserExtention
{
    public static UserOutputDto ToUserOutputDto(this AppUser user)
    {
        return new UserOutputDto()
        {
            Name = user.UserName!,
            Email = user.Email!,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address
        };
    }
}
