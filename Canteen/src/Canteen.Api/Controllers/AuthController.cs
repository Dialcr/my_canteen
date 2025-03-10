﻿

using Canteen.DataAccess.Entities;
using Canteen.DataAccess.Enums;
using Canteen.DataAccess.Settings;
using Canteen.Services.Abstractions;
using Canteen.Services.Dto.User;
using Canteen.Services.Security;
using Canteen.Services.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services.Dtos;

namespace Canteen.Controllers;

[ApiController]
[Route("api/[Controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly TokenUtil _tokenUtil;
    private readonly IUserServices _userServicers;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        TokenUtil tokenUtil,
        IUserServices userServicers,
        IHttpContextAccessor httpContextAccessor
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenUtil = tokenUtil;
        _userServicers = userServicers;
        _httpContextAccessor = httpContextAccessor;
    }

    [HttpPost]
    [Route("signin")]
    [AllowAnonymous]
    public async Task<IActionResult> Signin([FromBody] UserSignIn userSignIn)
    {
        var result = await _userServicers.LoginAsync(userSignIn.userEmail, userSignIn.UserPassword);
        if (result.TryPickT0(out var error, out var response))
        {
            return BadRequest(error);
        }
        return Ok(response);
    }

    [HttpPost]
    [Route("registrer")]
    //[Authorize(Roles = "ADMIN")]
    [AllowAnonymous]
    public async Task<IActionResult> UserRegistrer([FromBody] UserIntputDto userIntputDto)
    {
        // var accountController = nameof(AccountController.ConfirmEmailToken);

        // var result = await _userServicers.CreateUserAsync(userIntputDto, Url, accountController);
        var result = await _userServicers.CreateUserAsync(userIntputDto);
        if (result.TryPickT0(out var error, out var response))
        {
            return BadRequest(error);
        }
        return Ok(response);
    }

    [HttpPatch]
    [Route("editUser")]
    [Authorize(Roles = $"{nameof(RoleNames.CLIENT)},{nameof(RoleNames.ADMIN)}")]
    public async Task<IActionResult> EditUser([FromBody] UserIntputDto userIntputDto)
    {
        var result = await _userServicers.EditUser(userIntputDto);
        if (result.TryPickT0(out var error, out var response))
        {
            return BadRequest(error);
        }
        return Ok(response);
    }

    // [HttpPost]
    // [Route("recovery/password")]
    // [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    // [ProducesResponseType(typeof(ResponseErrorDto), StatusCodes.Status400BadRequest)]
    // [AllowAnonymous]
    // public async Task<IActionResult> RecoveryPassword(string email)
    // {
    //     var result = await _userServicers.RecoveryPassword(email);
    //     if (result.TryPickT0(out var error, out var response))
    //     {
    //         return BadRequest(error);
    //     }
    //     return Ok(response);
    // }
}
