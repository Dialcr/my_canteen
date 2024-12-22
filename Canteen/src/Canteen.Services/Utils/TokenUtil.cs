

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Canteen.DataAccess.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Canteen.DataAccess.Settings;

public class TokenUtil
{
    readonly UserManager<AppUser> _userManager;
    readonly JwtSettings _jwtSettings;

    public TokenUtil(UserManager<AppUser> userManager, IOptions<JwtSettings> jwtsettings)
    {
        _userManager = userManager;
        _jwtSettings = jwtsettings.Value;
    }

    public async Task<string> GenerateTokenAsync(AppUser user)
    {
        string? role = null;

        var userRoles = await _userManager.GetRolesAsync(user) ?? new List<string>();

        if (userRoles.ToList().Count == 0)
            role = RoleNames.Client;
        else
            role = userRoles.ToList().First().ToUpper();

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, $"{user.Id}"),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(ClaimTypes.Sid, $"{user.Id}"),
            new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.Role, role ?? RoleNames.Client),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.Now.AddMinutes(_jwtSettings.LifeTimeToken);
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            signingCredentials: creds,
            expires: expires
        );
        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
        return jwtToken;
    }

    public int GetUserIdFromToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.Key);

        try
        {
            tokenHandler.ValidateToken(
                token,
                new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidAudience = _jwtSettings.Audience,
                    LifetimeValidator = (notBefore, expires, securityToken, validationParameters) =>
                    {
                        return expires > DateTime.UtcNow;
                    },
                    ValidateLifetime = true,
                },
                out var validatedToken
            );

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userId = jwtToken.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sub).Value;
            //var userId2 = jwtToken.Claims.First(x => x.Type == ClaimTypes.Sid).Value;
            //var role = jwtToken.Claims.First(x => x.Type == ClaimTypes.Role).Value;

            return int.Parse(userId);
        }
        catch (Exception e)
        {
            return 0;
        }
    }
}
