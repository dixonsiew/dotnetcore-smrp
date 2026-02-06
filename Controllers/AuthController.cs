using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using smrp.Dtos;
using smrp.Services;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace smrp.Controllers
{
    [Route("")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TokenService tokenService;
        private readonly UserService userService;

        public AuthController(IConfiguration config, IDbConnection con)
        {
            tokenService = new TokenService(config);
            userService = new UserService(con);
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
            return Results.Json(new
            {
                type = "bearer",
                token = token,
                refresh_token = refreshToken,
            });
        }

        [HttpGet("api/current-user")]
        [Authorize]
        public async Task<IResult> UserDetails()
        {
            IResult res = Results.Json(new
            {
                statusCode = StatusCodes.Status404NotFound,
                message = "User not found",
            }, statusCode: StatusCodes.Status404NotFound);

            var userClaimsPrincipal = User;
            var userId = userClaimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return res;
            }

            int id = Convert.ToInt32(userId);
            var user = await userService.FindById(id);
            if (user == null)
            {
                return res;
            }

            return Results.Json(new
            {
                id = user.Id,
                username = user.Username,
                first_name = user.FirstName,
                last_name = user.LastName,
                roles = user.Roles,
            });
        }
    }
}
