using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using smrp.Models;
using smrp.Services;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace smrp.Controllers
{
    [Route("app-user")]
    [ApiController]
    public class AppUserController : ControllerBase
    {
        private UserService sv;

        public AppUserController(IDbConnection con)
        {
            sv = new UserService(con);
        }

        [HttpGet("me")]
        [Authorize]
        public IResult GetMe()
        {
            var userClaimsPrincipal = User;
            var userId = userClaimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = userClaimsPrincipal.FindFirst(ClaimTypes.Name)?.Value;
            var jti = userClaimsPrincipal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            return Results.Ok(new { userId, username, jti });
        }
    }
}
