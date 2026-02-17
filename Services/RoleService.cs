using Dapper;
using smrp.Models;
using smrp.Utils;

namespace smrp.Services
{
    public class RoleService
    {
        private readonly DefaultConnection ctx;
        private readonly ILogger<RoleService> logger;

        public RoleService(DefaultConnection c, ILogger<RoleService> log)
        {
            ctx = c;
            logger = log;
        }

        public async Task<List<Role>> FindAllAsync(string sortBy, string sortDir)
        {
            List<Role> lx = new List<Role>();
            using var conn = ctx.CreateConnection();
            var q = await conn.QueryAsync(@$"select id, name from role order by {sortBy} {sortDir}");
            lx = Role.List(q);

            return lx;
        }

        public async Task<Role?> FindByIdAsync(long id)
        {
            Role? role = null;
            using var conn = ctx.CreateConnection();
            var q = await conn.QuerySingleOrDefaultAsync(@"select id, name from role where id = @id limit 1", new { id });
            if (q != null)
            {
                role = Role.Single(q);
            }

            return role;
        }
    }
}
