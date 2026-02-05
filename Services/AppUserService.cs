using Dapper;
using smrp.Models;
using System.Data;

namespace smrp.Services
{
    public class AppUserService
    {
        private readonly IDbConnection conn;

        public AppUserService(IDbConnection con)
        {
            conn = con;
        }

        public async Task<List<AppUser>> FindAllUserAsync()
        {
            List<AppUser> lx = new List<AppUser>();
            var q = await conn.QueryAsync(@"select id, active, first_name, last_name, password, username, last_login from app_user");
            lx = AppUser.GetQ(q).ToList();
            return lx;
        }
    }
}
