using Dapper;
using smrp.Models;
using smrp.Utils;

namespace smrp.Services
{
    public class CommonSetupService
    {
        private readonly DefaultConnection ctx;

        public CommonSetupService(DefaultConnection c)
        {
            ctx = c;
        }

        public async Task<CommonSetup?> FindByIdAsync(long id, string table)
        {
            CommonSetup? o = null;
            using var conn = ctx.CreateConnection();
            conn.Open();
            var q = await conn.QuerySingleOrDefaultAsync(@$"select id, code, ""desc"", ref reff, created_by, created_date, modified_by, modified_date, deleted, deleted_by, deleted_date from {table} where id = @id limit 1", new { id });
            if (q != null)
            {
                o = CommonSetup.Single(q);
            }

            return o;
        }

        public async Task<CommonSetup?> FindByDescAsync(string desc, string table)
        {
            CommonSetup? o = null;
            using var conn = ctx.CreateConnection();
            conn.Open();
            var q = await conn.QuerySingleOrDefaultAsync(@$"select t.id, t.code, t.desc, t.ref reff, t.created_by, t.created_date, t.modified_by, t.modified_date, t.deleted, t.deleted_by, t.deleted_date from {table} t where lower(t.desc) = lower(@desc) limit 1", new { desc });
            if (q != null)
            {
                o = CommonSetup.Single(q);
            }

            return o;
        }

        public async Task<List<CommonSetup>> FindAllAsync(string table, int offset, int limit, string sortBy, string sortDir)
        {
            List<CommonSetup> lx = new List<CommonSetup>();
            using var conn = ctx.CreateConnection();
            conn.Open();

            if (limit > 0)
            {
                var q = await conn.QueryAsync($@"select t.id, t.code, t.desc, t.ref reff, t.created_by, t.created_date, t.modified_by, t.modified_date, t.deleted, t.deleted_by, t.deleted_date from {table} t where t.deleted is not true order by ""{sortBy}"" {sortDir} offset @offset limit @limit", )
            }
            var q = await conn.QueryAsync(@$"select t.id, t.username, t.first_name, t.last_name, t.password, t.last_login from app_user t order by {sortBy} {sortDir} offset @offset limit @limit", new { offset = offset, limit = limit });
            lx = CommonSetup.List(q);

            return lx;
        }
    }
}
