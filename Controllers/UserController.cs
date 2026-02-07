using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using smrp.Dtos;
using smrp.Models;
using smrp.Services;
using smrp.Utils;
using System.Data;

namespace smrp.Controllers
{
    [Route("api")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService userService;
        private readonly RoleService roleService;

        public UserController(IDbConnection con)
        {
            userService = new UserService(con);
            roleService = new RoleService(con);
        }

        [HttpGet("users")]
        [Authorize]
        public async Task<IResult> List(
            [FromQuery(Name = "_page")] string page = "1", 
            [FromQuery(Name = "_limit")] string limit = "20",
            [FromQuery(Name = "sort")] string sorts = "")
        {
            string sortBy = "username";
            string sortDir = "asc";

            if (sorts != "")
            {
                var lis = sorts.Split('$') ?? [];
                string s = lis[0];
                var arr = s.Split(':') ?? [];
                sortBy = arr[0];
                sortDir = arr[1];
            }

            var total = await userService.CountAsync();
            var pg = new Pager(total, Convert.ToInt32(page), Convert.ToInt32(limit));
            var lx = await userService.FindAllAsync(pg.LowerBound, pg.PageSize, sortBy, sortDir);
            Response.Headers.Append(Constants.X_TOTAL_COUNT, total.ToString());
            Response.Headers.Append(Constants.X_TOTAL_PAGE, pg.TotalPages.ToString());
            return Results.Ok(lx);
        }

        [HttpPost("users")]
        [Authorize]
        public async Task<IResult> SearchList(
            KeywordDto data,
            [FromQuery(Name = "_page")] string page = "1",
            [FromQuery(Name = "_limit")] string limit = "20",
            [FromQuery(Name = "sort")] string sorts = "")
        {
            string key = $"%{data.Keyword ?? ""}%";
            string sortBy = "username";
            string sortDir = "asc";

            if (sorts != "")
            {
                var lis = sorts.Split('$') ?? [];
                string s = lis[0];
                var arr = s.Split(':') ?? [];
                sortBy = arr[0];
                sortDir = arr[1];
            }

            var total = await userService.CountByKeywordAsync(key);
            var pg = new Pager(total, Convert.ToInt32(page), Convert.ToInt32(limit));
            var lx = await userService.FindByKeywordAsync(key, pg.LowerBound, pg.PageSize, sortBy, sortDir);
            Response.Headers.Append(Constants.X_TOTAL_COUNT, total.ToString());
            Response.Headers.Append(Constants.X_TOTAL_PAGE, pg.TotalPages.ToString());
            return Results.Ok(lx);
        }

        [HttpPost("user")]
        [Authorize]
        public async Task<IResult> Create(UserDto data)
        {
            string username = data.Username;
            bool b = await userService.ExistsByUsernameAsync(username);
            if (b)
            {
                return Results.Json(new
                {
                    statusCode = StatusCodes.Status400BadRequest,
                    message = "A user with that username already exists",
                }, statusCode: StatusCodes.Status400BadRequest);
            }

            var role = await roleService.FindByIdAsync(data.RoleId);
            if (role == null)
            {
                return Results.Json(new
                {
                    statusCode = StatusCodes.Status404NotFound,
                    message = "Role not found",
                }, statusCode: StatusCodes.Status404NotFound);
            }

            var o = new User
            {
                Username = data.Username,
                Password = data.Password,
                FirstName = data.Firstname,
                LastName = data.Lastname ?? "",
                Roles = [role],
            };
            await userService.SaveAsync(o);
            return Results.Ok(new
            {
                success = 1,
            });
        }

        [HttpGet("user/{id}")]
        [Authorize]
        public async Task<IResult> Edit(long id)
        {
            var o = await userService.FindByIdAsync(id);
            if (o == null)
            {
                return Results.Json(new
                {
                    statusCode = StatusCodes.Status404NotFound,
                    message = "User not found",
                }, statusCode: StatusCodes.Status404NotFound);
            }

            return Results.Ok(o);
        }
    }
}
