using Dapper;
using smrp.Models;
using smrp.Utils;
using System.Data;

namespace smrp.Services
{
    public class RoleService
    {
        private readonly DefaultConnection ctx;

        public RoleService(DefaultConnection c)
        {
            ctx = c;
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
