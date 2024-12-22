

using Canteen.DataAccess;
using Canteen.DataAccess.Enums;
using Canteen.DataAccess.Settings;
using Canteen.Services.Abstractions;
using Canteen.Services.Dto;
using Canteen.Services.Dto.User;
using Canteen.Services.Security;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using OneOf.Types;

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
        var result = await _userManager!.CreateAsync(
            new AppUser()
            {
                UserName = userIntputDto.Name,
                Email = userIntputDto.Email,
                EstablishmentId = userIntputDto.EstablishmentId,
            },
            userIntputDto.Password
        );

        if (!result.Succeeded)
        {
            return Error("error registering",
                $"error registering",
                400);
        }
        var user = await _userManager.FindByNameAsync(userIntputDto.Name);
        // var token = await _userManager.GenerateEmailConfirmationTokenAsync(user!);


        return Error("error registering", $"error registering", 400);
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

        var userOutput = user.ToUserOutputDto();
        var role = await _userManager.GetRolesAsync(user);
        userOutput.Role = role[0];
        return userOutput;
    }

    public async Task<OneOf<ResponseErrorDto, AuthResponseDtoOutput>> LoginAsync(
        string username,
        string userPassword
    )
    {
        var user = _userManager.Users.FirstOrDefault(x => x.UserName == username);
        if (user is not null)
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
                        Role = role[0]
                    },
                    Authentication = new Jwt() { AccessToken = token },
                    UserId = user.Id
                };
            }

            return Error("password incorrect", "password incorrect", 400);
        }
        return Error("user not found", "user not found", 400);
    }

    public OneOf<ResponseErrorDto, List<AppUser>> ListUser()
    {
        var response = _userManager.Users.ToList();
        if (response.IsNullOrEmpty() || response.Count > 0)
        {
            return response;
        }
        return Error("Not user registered", "Not user registered", 400);
    }

    public async Task<OneOf<ResponseErrorDto, AppUser>> GetUserById(int userId)
    {
        var response = await _userManager
            .Users.Include(x => x.Establishment)
            .SingleOrDefaultAsync(x => x.Id == userId);
        if (response is not null)
        {
            return response;
        }
        return Error("user not found", "user not found", 400);
    }

    public async Task<OneOf<ResponseErrorDto, AppUser>> GetUserByEmail(string userEmail)
    {
        var response = await _userManager.FindByEmailAsync(userEmail);
        if (response is not null)
        {
            return response;
        }
        return Error("user not found", "user not found", 400);
    }

    public async Task<OneOf<ResponseErrorDto, AppUser>> GetUserByUserName(string userName)
    {
        var response = await _userManager.FindByNameAsync(userName);
        if (response is not null)
        {
            return response;
        }
        return Error("user not found", "user not found", 400);
    }

    public async Task<OneOf<ResponseErrorDto, AppUser>> GetUserByUserNameOrEmail(
        string userNameOrEmail
    )
    {
        var response =
            await _userManager.FindByNameAsync(userNameOrEmail)
            ?? await _userManager.FindByEmailAsync(userNameOrEmail);
        if (response is not null)
        {
            return response;
        }
        return Error("user not found", "user not found", 400);
    }

    public async Task<OneOf<ResponseErrorDto, AppUser>> EditUser(
        UserIntputDto userIntputDto,
        int userId
    )
    {
        var result = GetUserById(userId);

        if (result.Result.TryPickT0(out var error, out var response))
        {
            return error;
        }

        try
        {
            response.Email = userIntputDto.Email;
            response.EstablishmentId = userIntputDto.EstablishmentId;
            await context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        return response;
    }

    public async Task<OneOf<ResponseErrorDto, AppUser>> ResetPassword(
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
            return user;
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
