using Canteen.DataAccess.Settings;
using Canteen.Services.Dto;
using Canteen.Services.Dto.User;
using Canteen.Services.Security;

namespace Canteen.Services.Abstractions;

public interface IUserServices
{
    Task<OneOf<ResponseErrorDto, UserOutputDto>> CreateUserAsync(UserIntputDto userIntputDto);
    Task<OneOf<ResponseErrorDto, UserOutputDto>> EditUser(UserIntputDto userIntputDto);
    Task<OneOf<ResponseErrorDto, AuthResponseDtoOutput>> LoginAsync(string userEmail, string userPassword);
    OneOf<ResponseErrorDto, IEnumerable<UserOutputDto>> ListUser();
    Task<OneOf<ResponseErrorDto, UserOutputDto>> GetUserById(int userId);
    Task<OneOf<ResponseErrorDto, UserOutputDto>> GetUserByEmail(string userEmail);
    Task<OneOf<ResponseErrorDto, UserOutputDto>> GetUserByUserName(string userName);
    Task<OneOf<ResponseErrorDto, UserOutputDto>> GetUserByUserNameOrEmail(string userNameOrEmail);
    Task<OneOf<ResponseErrorDto, UserOutputDto>> EditUser(UserIntputDto userIntputDto, int userId);
    Task<OneOf<ResponseErrorDto, UserOutputDto>> ResetPassword(string userEmail, string newPassword, string token);
    Task<bool> confirmEmail(string token, string userName);
    bool ConfirmEmailToken(string token, string userName);
}
