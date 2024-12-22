using Canteen.DataAccess.Settings;
using Canteen.Services.Dto;
using Canteen.Services.Dto.User;
using Canteen.Services.Security;

namespace Canteen.Services.Abstractions;

public interface IUserServices
{
    Task<OneOf<ResponseErrorDto, UserOutputDto>> CreateUserAsync(UserIntputDto userIntputDto);
    Task<OneOf<ResponseErrorDto, UserOutputDto>> EditUser(UserIntputDto userIntputDto);
    Task<OneOf<ResponseErrorDto, AuthResponseDtoOutput>> LoginAsync(string username, string userPassword);
    OneOf<ResponseErrorDto, List<AppUser>> ListUser();
    Task<OneOf<ResponseErrorDto, AppUser>> GetUserById(int userId);
    Task<OneOf<ResponseErrorDto, AppUser>> GetUserByEmail(string userEmail);
    Task<OneOf<ResponseErrorDto, AppUser>> GetUserByUserName(string userName);
    Task<OneOf<ResponseErrorDto, AppUser>> GetUserByUserNameOrEmail(string userNameOrEmail);
    Task<OneOf<ResponseErrorDto, AppUser>> EditUser(UserIntputDto userIntputDto, int userId);
    Task<OneOf<ResponseErrorDto, AppUser>> ResetPassword(string userEmail, string newPassword, string token);
    Task<bool> confirmEmail(string token, string userName);
    bool ConfirmEmailToken(string token, string userName);
}
