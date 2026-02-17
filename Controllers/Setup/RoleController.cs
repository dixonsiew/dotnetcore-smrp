using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using smrp.Services;

namespace smrp.Controllers.Setup
{
    [Tags("Setup/Role")]
    [Route("api")]
    [ApiController]
    [Authorize]
    public class RoleController : ControllerBase
    {
        private readonly RoleService roleService;

        public RoleController(ILogger<RoleController> logger, RoleService rs)
        {
            roleService = rs;
        }

        [HttpGet("lookup/groups")]
        public async Task<IResult> LookupList()
        {
            var ls = await roleService.FindAllAsync("name", "asc");
            return Results.Json(ls);
        }
    }
}
