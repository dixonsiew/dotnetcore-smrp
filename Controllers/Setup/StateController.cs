using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using smrp.Dtos;
using smrp.Models;
using smrp.Services;
using smrp.Utils;
using System.Security.Claims;

namespace smrp.Controllers.Setup
{
    [Tags("Setup/State")]
    [Route("api")]
    [ApiController]
    [Authorize]
    public class StateController : ControllerBase
    {
        private readonly CommonSetupService commonSetupService;
        private const string table = "state";

        public StateController(DefaultConnection conn, ILogger<StateController> logger)
        {
            commonSetupService = new CommonSetupService(conn, logger);
        }

        [HttpGet("lookup/states")]
        public async Task<IResult> LookupList()
        {
            var ls = await commonSetupService.FindAllAsync(table, 0, 0, "", "");
            return Results.Json(ls);
        }

        [HttpGet("states")]
        public async Task<IResult> List(
            [FromQuery(Name = "_page")] string page = "1",
            [FromQuery(Name = "_limit")] string limit = "20",
            [FromQuery(Name = "sort")] string sorts = "")
        {
            string sortBy = "code";
            string sortDir = "asc";

            if (sorts != "")
            {
                var lis = sorts.Split('$') ?? [];
                string s = lis[0];
                var arr = s.Split(':') ?? [];
                sortBy = arr[0];
                sortDir = arr[1];
            }

            var total = await commonSetupService.CountAsync(table);
            var pg = new Pager(total, Convert.ToInt32(page), Convert.ToInt32(limit));
            var lx = await commonSetupService.FindAllAsync(table, pg.LowerBound, pg.PageSize, sortBy, sortDir);
            Response.Headers.Append(Constants.X_TOTAL_COUNT, total.ToString());
            Response.Headers.Append(Constants.X_TOTAL_PAGE, pg.TotalPages.ToString());
            return Results.Json(lx);
        }

        [HttpPost("states")]
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

            var total = await commonSetupService.CountByKeywordAsync(table, key);
            var pg = new Pager(total, Convert.ToInt32(page), Convert.ToInt32(limit));
            var lx = await commonSetupService.FindByKeywordAsync(key, pg.LowerBound, pg.PageSize, sortBy, sortDir, table);
            Response.Headers.Append(Constants.X_TOTAL_COUNT, total.ToString());
            Response.Headers.Append(Constants.X_TOTAL_PAGE, pg.TotalPages.ToString());
            return Results.Json(lx);
        }

        [HttpPost("state")]
        public async Task<IResult> Create(CommonSetupDto data)
        {
            var userClaimsPrincipal = User;
            var id = userClaimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (id == null)
            {
                return ApiResult.UserNotFound;
            }

            var userId = Convert.ToInt32(id);
            var o = new CommonSetup
            {
                Code = data.Code,
                Desc = data.Desc,
                Ref = data.Ref,
                CreatedBy = userId,
            };
            await commonSetupService.SaveAsync(o, table);
            return Results.Json(new
            {
                success = 1,
            });
        }

        [HttpGet("state/{id}")]
        public async Task<IResult> Edit(long id)
        {
            var o = await commonSetupService.FindByIdAsync(id, table);
            if (o == null)
            {
                return Results.Json(new
                {
                    statusCode = StatusCodes.Status404NotFound,
                    message = "Record not found",
                }, statusCode: StatusCodes.Status404NotFound);
            }

            return Results.Json(o);
        }

        [HttpPut("state/{id}")]
        public async Task<IResult> Update(CommonSetupDto data, long id)
        {
            var userClaimsPrincipal = User;
            var ids = userClaimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (ids == null)
            {
                return ApiResult.UserNotFound;
            }

            var userId = Convert.ToInt32(ids);
            var o = await commonSetupService.FindByIdAsync(id, table);
            if (o == null)
            {
                return Results.Json(new
                {
                    statusCode = StatusCodes.Status404NotFound,
                    message = "Record not found",
                }, statusCode: StatusCodes.Status404NotFound);
            }

            o.Code = data.Code;
            o.Desc = data.Desc;
            o.Ref = data.Ref;
            o.ModifiedBy = userId;
            await commonSetupService.UpdateAsync(o, table);
            return Results.Json(new
            {
                success = 1,
            });
        }

        [HttpDelete("state/{id}")]
        public async Task<IResult> Delete(CommonSetupDto data, long id)
        {
            var userClaimsPrincipal = User;
            var ids = userClaimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (ids == null)
            {
                return ApiResult.UserNotFound;
            }

            var userId = Convert.ToInt32(ids);
            var o = await commonSetupService.FindByIdAsync(id, table);
            if (o == null)
            {
                return Results.Json(new
                {
                    statusCode = StatusCodes.Status404NotFound,
                    message = "Record not found",
                }, statusCode: StatusCodes.Status404NotFound);
            }

            await commonSetupService.DeleteByIdAsync(id, userId, table);
            return Results.Json(new
            {
                success = 1,
            });
        }
    }
}
