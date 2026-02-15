using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using smrp.Services;
using smrp.Utils;

namespace smrp.Controllers.Setup
{
    [Tags("Setup/Role")]
    [Route("api")]
    [ApiController]
    [Authorize]
    public class RoleController : ControllerBase
    {
        private readonly RoleService roleService;
        private const string table = "city";

        public RoleController(DefaultConnection conn, ILogger<RoleController> logger)
        {
            roleService = new RoleService(conn, logger);
        }

        [HttpGet("lookup/groups")]
        public async Task<IResult> LookupList()
        {
            var ls = await roleService.FindAllAsync("name", "asc");
            return Results.Json(ls);
        }
    }
}
