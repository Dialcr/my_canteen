
namespace Canteen.Services.Dto.User;

public class UserOutputDto
{
    public string Name { get; set; } = "";

    public string Email { get; set; } = "";

    public string Role { get; set; } = "";
}

public static class UserExtention
{
    public static UserOutputDto ToUserOutputDto(this AppUser user)
    {
        return new UserOutputDto()
        {
            Name = user.UserName!,
            Email = user.Email!,
        };
    }
}
