using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using smrp.Dtos;
using smrp.Services;
using smrp.Utils;
using System.Security.Claims;

namespace smrp.Controllers
{
    [Route("")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TokenService tokenService;
        private readonly UserService userService;

        public AuthController(ILogger<AuthController> logger, TokenService ts, UserService us)
        {
            tokenService = ts;
            userService = us;
        }

        [HttpPost("o/token")]
        [AllowAnonymous]
        public async Task<IResult> Login(LoginDto data)
        {
            try
            {
                var user = await userService.FindByUsernameAsync(data.Username);
                if (user == null)
                {
                    throw new Exception();
                }

                bool valid = false;
                valid = userService.ValidateCredentials(user, data.Password);
                if (!valid)
                {
                    throw new Exception();
                }

                await userService.UpdateLastLoginAsync(user.Id);
                string token = tokenService.GenerateAccessToken(user);
                string refreshToken = tokenService.GenerateRefreshToken(user);
                return Results.Json(new
                {
                    type = "bearer",
                    token,
                    refresh_token = refreshToken,
                });
            }
            
            catch (Exception)
            {
                throw new UnauthorizedAccessException("Invalid Credentials");
            }
        }

        [HttpGet("api/current-user")]
        [Authorize]
        public async Task<IResult> UserDetails()
        {
            try
            {
                var userClaimsPrincipal = User;
                var userId = userClaimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return ApiResult.UserNotFound;
                }

                int id = Convert.ToInt32(userId);
                var user = await userService.FindByIdAsync(id);
                if (user == null)
                {
                    return ApiResult.UserNotFound;
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
            
            catch (Exception)
            {
                return ApiResult.UserNotFound;
            }
        }

        [HttpPost("api/change-password")]
        [Authorize]
        public async Task<IResult> ChangePassword(ChangePasswordDto data)
        {
            try
            {
                var userClaimsPrincipal = User;
                var userId = userClaimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return ApiResult.UserNotFound;
                }

                long id = Convert.ToInt64(userId);
                var user = await userService.FindByIdAsync(id);
                if (user == null)
                {
                    return ApiResult.UserNotFound;
                }

                if (data.Password != data.ConfirmPassword)
                {
                    throw new Exception("Confirm Password does not match");
                }

                user.Password = data.Password;
                await userService.UpdatePasswordAsync(user);
                return Results.Json(new
                {
                    success = 1,
                });
            }

            catch (Exception ex)
            {
                throw new BadHttpRequestException(ex.Message);
            }
        }

        [HttpGet("rapidoc")]
        [AllowAnonymous]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IResult Rapidoc()
        {
            return Results.Redirect("/rapidoc.html", permanent: true);
        }

        [HttpGet("swagger")]
        [AllowAnonymous]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IResult SwaggerUI()
        {
            return Results.Redirect("/swagger-ui/index.html", permanent: true);
        }

        [HttpGet("test")]
        [AllowAnonymous]
        public IResult Test()
        {
            throw new BadHttpRequestException("This is a test exception for global error handling.");
        }
    }
}
