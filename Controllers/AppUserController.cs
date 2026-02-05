using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using smrp.Models;
using smrp.Services;
using System.Data;

namespace smrp.Controllers
{
    [Route("app-user")]
    [ApiController]
    public class AppUserController : ControllerBase
    {
        private AppUserService sv;

        public AppUserController(IDbConnection con)
        {
            sv = new AppUserService(con);
        }

        [HttpGet("list")]
        public async Task<List<AppUser>> GetUsers()
        {
            return await sv.FindAllUserAsync();
        }

    }
}
