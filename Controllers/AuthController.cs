using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using smrp.Dtos;
using smrp.Services;

namespace smrp.Controllers
{
    [Route("")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TokenService tokenService;
        private readonly UserService userService;

        public AuthController(IConfiguration config)
        {
            tokenService = new TokenService(config);
        }

        [HttpPost("o/token")]
        [AllowAnonymous]
        public async Task<IResult> Login(LoginDto data)
        {
            var mx = new { statusCode = StatusCodes.Status401Unauthorized, message = "Invalid Credentials" };

            var user = await userService.FindByUsername(data.Username);
            if (user == null)
            {
                return Results.Json(mx, statusCode: StatusCodes.Status401Unauthorized);
            }

            bool valid = false;
            valid = userService.ValidateCredentials(user, data.Password);
            if (!valid)
            {
                return Results.Json(mx, statusCode: StatusCodes.Status401Unauthorized);
            }

            await userService.UpdateLastLogin(user.Id);
            string token = tokenService.GenerateAccessToken(user);
            string refreshToken = tokenService.GenerateRefreshToken(user);
        }
    }
}
