using DemoAPICustom.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DemoAPICustom.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly SignInManager<IdentityUser> signInManager;
    private readonly UserManager<IdentityUser> userManager;
    private readonly IConfiguration configuration;

    public AccountController(
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager,
        IConfiguration configuration)
    {
        this.signInManager = signInManager;
        this.userManager = userManager;
        this.configuration = configuration;
    }


    [HttpPost]
    [AllowAnonymous]
    [Route("register")]
    public async Task<ActionResult> Register(
        [FromBody] DemoAPICustom.Models.RegisterRequest registerRequest) { 
    
        IdentityUser identityUser = new IdentityUser
        {
            UserName = registerRequest.Email,
            Email = registerRequest.Email
        };

        var result = 
            await userManager.CreateAsync(identityUser, registerRequest.Password);

        if (result.Succeeded)
        {
            return StatusCode(
                StatusCodes.Status201Created,
                new { result.Succeeded });
        }
        else
        {
            return StatusCode(
                StatusCodes.Status400BadRequest,
                new { result.Errors });
        }
    }

    [HttpPost]
    [Route("login")]
    [AllowAnonymous]
    public async Task<IActionResult> SignIn(
        [FromBody] DemoAPICustom.Models.RegisterRequest user)
    {
        var signinResult = await signInManager.PasswordSignInAsync(
            user.Email, user.Password, true, false);

        if(signinResult.Succeeded)
        {
            var identityUser = 
                await userManager.FindByEmailAsync(user.Email);

            // generazione del token
            if(identityUser != null)
            {
                var jwt = await GeneraJSONWebToken(identityUser);

                return Ok(jwt);
            } else
            {
                return Unauthorized();
            }

        } else
        {
            return Unauthorized();
        }

    }

    [NonAction]
    [ApiExplorerSettings(IgnoreApi = true)]
    private async Task<string> GeneraJSONWebToken(IdentityUser user)
    {
        SymmetricSecurityKey key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:SecurityKey"]!));

        SecurityKey securityKey = key;

        SigningCredentials credentials = new 
            SigningCredentials(key,            
          SecurityAlgorithms.HmacSha256);

        IList<string> roles = await userManager.GetRolesAsync(user);
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim("CodiceInterno", "xxx666")
        };
        claims = claims.Union(
            roles.Select(role => new Claim(ClaimTypes.Role, role))).ToList();

        JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

    }
}
