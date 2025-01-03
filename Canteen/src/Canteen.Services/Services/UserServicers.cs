

using Canteen.DataAccess;
using Canteen.DataAccess.Enums;
using Canteen.DataAccess.Settings;
using Canteen.Services.Abstractions;
using Canteen.Services.Dto;
using Canteen.Services.Dto.User;
using Canteen.Services.Security;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Identity;

namespace Canteen.Services.Services;

public class UserServicers(UserManager<AppUser> _userManager,
        SignInManager<AppUser> _signInManager,
        TokenUtil _tokenUtil,
        EntitiesContext context) : CustomServiceBase(context), IUserServices
{
    // private readonly UserManager<AppUser> _userManager;
    // private readonly SignInManager<AppUser> _signInManager;
    // private readonly TokenUtil _tokenUtil;
    // // private readonly MailSettings _mailSettings;
    // // private readonly EmailService _emailServices;

    // public UserServicers(
    //     UserManager<AppUser> _userManager,
    //     SignInManager<AppUser> _signInManager,
    //     TokenUtil _tokenUtil,
    //     EntitiesContext context
    // // IOptions<MailSettings> mailSettings,
    // // EmailService emailServices
    // )
    //     : base(context)
    // {
    //     _userManager = userManager;
    //     _signInManager = signInManager;
    //     _tokenUtil = tokenUtil;
    //     // _mailSettings = mailSettings.Value;
    //     // _emailServices = emailServices;
    // }

    public async Task<OneOf<ResponseErrorDto, UserOutputDto>> CreateUserAsync(
        UserIntputDto userIntputDto
    // IUrlHelper urlHelper,
    // string accountController
    )
    {
        var establishment = new Establishment();
        if (userIntputDto.EstablishmentId is not null)
        {
            establishment = context.Establishments.FirstOrDefault(x => x.Id == userIntputDto.EstablishmentId);
            if (establishment is null)
            {
                return Error("Establishment not found", $"Establishment no found", 400);
            }
        }

        var result = await _userManager!.CreateAsync(
            new AppUser()
            {
                Name = userIntputDto.Name,
                Email = userIntputDto.Email,
                EstablishmentId = userIntputDto.EstablishmentId,
                LastName = userIntputDto.LastName,
                PhoneNumber = userIntputDto.PhoneNumber,
                Address = userIntputDto.Address
            },
            userIntputDto.Password
        );

        if (!result.Succeeded)
        {
            return Error("error registering",
                result.Errors.First().Description,
                400);
        }

        var user = await _userManager.FindByNameAsync(userIntputDto.Name);
        if (userIntputDto.EstablishmentId is not null)
        {
            await _userManager.AddToRoleAsync(user, RoleNames.Admin.ToUpper());
        }

        await _userManager.AddToRoleAsync(user, RoleNames.Client.ToUpper());

        // var token = await _userManager.GenerateEmailConfirmationTokenAsync(user!);
        var response = user!.ToUserOutputDto();
        var role = await _userManager.GetRolesAsync(user);
        response.Role = role;
        return response;

    }

    public async Task<OneOf<ResponseErrorDto, UserOutputDto>> EditUser(UserIntputDto userIntputDto)
    {
        var user = _userManager!.FindByNameAsync(userIntputDto.Name).Result;
        if (user is null)
        {
            return Error("user not found", $"User no found", 400);
        }

        user.Email = userIntputDto.Email;
        user.EstablishmentId = userIntputDto.EstablishmentId;
        user.PhoneNumber = userIntputDto.PhoneNumber;
        user.Address = userIntputDto.Address;
        user.LastName = userIntputDto.LastName;
        user.Name = userIntputDto.Name;

        var userOutput = user.ToUserOutputDto();
        var role = await _userManager.GetRolesAsync(user);
        userOutput.Role = role;
        return userOutput;
    }

    public async Task<OneOf<ResponseErrorDto, AuthResponseDtoOutput>> LoginAsync(
        string userEmail,
        string userPassword
    )
    {
        // var user = _userManager.Users.FirstOrDefault(x => x.UserName == username);
        var user = await _userManager.FindByEmailAsync(userEmail);
        if (user is not null)
        {
            try
            {

                var result = await _signInManager.PasswordSignInAsync(user, userPassword, false, false);

                if (result.Succeeded)
                {
                    var role = await _userManager.GetRolesAsync(user);
                    var token = await _tokenUtil.GenerateTokenAsync(user);
                    return new AuthResponseDtoOutput()
                    {
                        User = new UserOutputDto()
                        {
                            Name = user.UserName!,
                            Email = user.Email!,
                            Role = role
                        },
                        Authentication = new Jwt() { AccessToken = token },
                        UserId = user.Id
                    };
                }

                return Error("password incorrect", "password incorrect", 400);
            }
            catch (Exception e)
            {
                return Error($"{e.InnerException}", $"{e.Message}", 400);
            }
        }
        return Error("user not found", "user not found", 400);
    }

    public OneOf<ResponseErrorDto, IEnumerable<UserOutputDto>> ListUser()
    {
        var response = _userManager.Users.ToList();
        if (!response.IsNullOrEmpty() || response.Count > 0)
        {
            return response.Select(x => x.ToUserOutputDto()).ToList();
        }
        return Error("Not user registered", "Not user registered", 400);
    }

    public async Task<OneOf<ResponseErrorDto, UserOutputDto>> GetUserById(int userId)
    {
        var response = await _userManager
            .Users.Include(x => x.Establishment)
            .SingleOrDefaultAsync(x => x.Id == userId);
        if (response is not null)
        {
            return response.ToUserOutputDto();
        }
        return Error("user not found", "user not found", 400);
    }

    public async Task<OneOf<ResponseErrorDto, UserOutputDto>> GetUserByEmail(string userEmail)
    {
        var response = await _userManager.FindByEmailAsync(userEmail);
        if (response is not null)
        {
            return response.ToUserOutputDto();
        }
        return Error("user not found", "user not found", 400);
    }

    public async Task<OneOf<ResponseErrorDto, UserOutputDto>> GetUserByUserName(string userName)
    {
        var response = await _userManager.FindByNameAsync(userName);
        if (response is not null)
        {
            return response.ToUserOutputDto();
        }
        return Error("user not found", "user not found", 400);
    }

    public async Task<OneOf<ResponseErrorDto, UserOutputDto>> GetUserByUserNameOrEmail(
        string userNameOrEmail
    )
    {
        var response =
            await _userManager.FindByNameAsync(userNameOrEmail)
            ?? await _userManager.FindByEmailAsync(userNameOrEmail);
        if (response is not null)
        {
            return response.ToUserOutputDto();
        }
        return Error("user not found", "user not found", 400);
    }

    public async Task<OneOf<ResponseErrorDto, UserOutputDto>> EditUser(
        UserIntputDto userIntputDto,
        int userId
    )
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Error("user not found", "user not found", 400);
        }
        try
        {
            user.Email = userIntputDto.Email;
            user.EstablishmentId = userIntputDto.EstablishmentId;
            await context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        return user.ToUserOutputDto();
    }

    public async Task<OneOf<ResponseErrorDto, UserOutputDto>> ResetPassword(
        string userEmail,
        string newPassword,
        string token
    )
    {
        var user = await _userManager.FindByEmailAsync(userEmail);
        if (user is not null)
        {
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded)
            {
                return Error("Reset password failed", "Reset password failed", 400);

            }
            return user.ToUserOutputDto();
        }

        return Error("user not found", "user not found", 400);
    }

    //     public async Task<OneOf<ResponseErrorDto, string>> RecoveryPassword(string email)
    //     {
    //         try
    //         {
    //             var user = await _userManager.FindByEmailAsync(email);
    //             if (user is null)
    //             {
    //                 return Error("user not found", "user not found", 400);
    //             }
    //             var token = await _userManager.GeneratePasswordResetTokenAsync(user);
    //             var mailMessage = new MailMessage(_mailSettings.From, user.Email!);
    //             mailMessage.Subject = "Reset Password";

    //             string urlConToken = $"{_mailSettings.UrlWEBFront}?token={token}";

    // #if DEBUG
    //             var templatePath = "./../Services/Services/Template/RecoveryPasswordTemplate.html";
    // #else
    //             templatePath = Path.Combine(
    //                 Directory.GetCurrentDirectory(),
    //                 "./RecoveryPasswordTemplate.html"
    //             );
    // #endif
    //             var content = File.ReadAllText(templatePath);
    //             var urlFront = content.Replace("{{SupportEmailAddress}}", urlConToken);

    //             mailMessage.Body = urlFront;
    //             mailMessage.IsBodyHtml = true;
    //             _emailServices.SendEmail(mailMessage, token);
    //             return token;
    //         }
    //         catch (Exception e)
    //         {
    //             return new ResponseErrorDto() { ErrorCode = 404, ErrorMessage = e.Message };
    //         }
    //     }

    public async Task<bool> confirmEmail(string token, string userName)
    {
        bool esc = true;
        var user = _userManager.Users.FirstOrDefault(x => x.UserName == userName);
        if (user is not null)
        {
            var result = await _userManager.ConfirmEmailAsync(user!, token);
            if (!result.Succeeded)
            {
                esc = false;
            }
        }
        else
        {
            esc = false;
        }
        return esc;
    }

    public bool ConfirmEmailToken(string token, string userName)
    {
        bool esc = true;
        var user = _userManager.Users.FirstOrDefault(x => x.UserName == userName);
        if (user is not null)
        {
            var result = _userManager.ConfirmEmailAsync(user!, token);
            if (!result.Result.Succeeded)
            {
                esc = false;
            }
        }
        else
        {
            esc = false;
        }
        return esc;
    }
}
