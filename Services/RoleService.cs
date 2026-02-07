using Dapper;
using smrp.Models;
using System.Data;

namespace smrp.Services
{
    public class RoleService
    {
        private readonly IDbConnection conn;

        public RoleService(IDbConnection con)
        {
            conn = con;
        }

        public async Task<List<Role>> FindAllAsync(string sortBy, string sortDir)
        {
            List<Role> lx = new List<Role>();
            var q = await conn.QueryAsync(@$"select id, name from role order by {sortBy} {sortDir}");
            lx = Role.List(q);
            return lx;
        }

        public async Task<Role?> FindById(long id)
        {
            Role? role = null;
            var q = await conn.QuerySingleOrDefaultAsync(@"select id, name from role where id = @id limit 1", new { id = id });
            if (q != null)
            {
                role = Role.Single(q);
            }

            return role;
        }
    }
}
