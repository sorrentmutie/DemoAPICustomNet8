using DemoAPICustom.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DemoAPICustom.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly UserManager<IdentityUser> userManager;

        public AccountController(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("register")]
        public async Task<ActionResult> Register(
            [FromBody] RegisterRequest registerRequest) { 
        
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
    }
}
