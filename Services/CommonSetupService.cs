using Dapper;
using smrp.Models;
using smrp.Utils;

namespace smrp.Services
{
    public class CommonSetupService
    {
        private readonly DefaultConnection ctx;
        private readonly ILogger logger;

        public CommonSetupService(DefaultConnection c, ILogger log)
        {
            ctx = c;
            logger = log;
        }

        public async Task<CommonSetup?> FindByIdAsync(long id, string table)
        {
            CommonSetup? o = null;
            try
            {
                using var conn = ctx.CreateConnection();
                var q = await conn.QuerySingleOrDefaultAsync(@$"select id, code, ""desc"", ref reff, created_by, created_date, modified_by, modified_date, deleted, deleted_by, deleted_date from {table} where id = @id limit 1", new { id });
                if (q != null)
                {
                    o = CommonSetup.Single(q);
                }
            }

            catch (Exception ex)
            {
                logger.LogError(ex, $"Error finding {table} by id {id}");
            }

            return o;
        }

        public async Task<CommonSetup?> FindByDescAsync(string desc, string table)
        {
            CommonSetup? o = null;
            try
            {
                using var conn = ctx.CreateConnection();
                var q = await conn.QuerySingleOrDefaultAsync(@$"select t.id, t.code, t.desc, t.ref reff, t.created_by, t.created_date, t.modified_by, t.modified_date, t.deleted, t.deleted_by, t.deleted_date from {table} t where lower(t.desc) = lower(@desc) limit 1", new { desc });
                if (q != null)
                {
                    o = CommonSetup.Single(q);
                }
            }
            
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error finding {table} by desc {desc}");
            }

            return o;
        }

        public async Task<List<CommonSetup>> FindAllAsync(string table, int offset, int limit, string sortBy, string sortDir)
        {
            List<CommonSetup> lx = new List<CommonSetup>();
            try
            {
                using var conn = ctx.CreateConnection();

                if (limit > 0)
                {
                    var q = await conn.QueryAsync(@$"select t.id, t.code, t.desc, t.ref reff, t.created_by, t.created_date, t.modified_by, t.modified_date, t.deleted, t.deleted_by, t.deleted_date from {table} t where t.deleted is not true order by ""{sortBy}"" {sortDir} offset @offset limit @limit", new { offset, limit });
                    lx = CommonSetup.List(q);
                }

                else
                {
                    var q = await conn.QueryAsync(@$"select t.id, t.code, t.desc, t.ref reff, t.created_by, t.created_date, t.modified_by, t.modified_date, t.deleted, t.deleted_by, t.deleted_date from {table} t where t.desc <> '' and t.deleted is not true order by t.code");
                    lx = CommonSetup.List(q);
                }
            }

            catch (Exception ex)
            {
                logger.LogError(ex, $"Error finding all {table}");
            }

            return lx;
        }

        public async Task<int> CountAsync(string table)
        {
            int q = 0;
            try
            {
                using var conn = ctx.CreateConnection();
                q = await conn.ExecuteScalarAsync<int>(@$"select count(id) from {table} t where t.deleted is not true");
            }
            
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error counting {table}");
            }

            return q;
        }

        public async Task<List<CommonSetup>> FindByKeywordAsync(string keyword, int offset, int limit, string sortBy, string sortDir, string table)
        {
            List<CommonSetup> lx = new List<CommonSetup>();
            try
            {
                using var conn = ctx.CreateConnection();
                var q = await conn.QueryAsync(@$"select t.id, t.code, t.desc, t.ref reff, t.created_by, t.created_date, t.modified_by, t.modified_date, t.deleted, t.deleted_by, t.deleted_date from {table} t where (t.code ilike @keyword or t.desc ilike @keyword or t.ref ilike @keyword) and t.deleted is not true order by ""{sortBy}"" {sortDir} offset @offset limit @limit", new { keyword, offset, limit });
                lx = CommonSetup.List(q);
            }

            catch (Exception ex)
            {
                logger.LogError(ex, $"Error finding {table} by keyword {keyword}");
            }

            return lx;
        }

        public async Task<int> CountByKeywordAsync(string keyword, string table)
        {
            int q = 0;
            try
            {
                using var conn = ctx.CreateConnection();
                q = await conn.ExecuteScalarAsync<int>(@$"select count(id) from {table} t where (t.code ilike @keyword or t.desc ilike @keyword or t.ref ilike @keyword) and t.deleted is not true", new { keyword });
            }
            
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error counting {table} by keyword {keyword}");
            }

            return q;
        }

        public async Task SaveAsync(CommonSetup o, string table)
        {
            try
            {
                using var conn = ctx.CreateConnection();
                var q = @$"insert into {table} (id, code, ""desc"", ref, created_by, created_date) values(nextval('{table}_id_seq'),@code,@desc,@reff,@createdby,now())";
                await conn.ExecuteAsync(q, new { code = o.Code, desc = o.Desc, reff = o.Ref, createdby = o.CreatedBy });
            }
            
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error saving {table} with code {o.Code}");
            }
        }

        public async Task UpdateAsync(CommonSetup o, string table)
        {
            try
            {
                using var conn = ctx.CreateConnection();
                var q = $@"update {table} set code = @code, ""desc"" = @desc, ref = @reff, modified_by = @modifiedby, modified_date = now() where id = @id";
                await conn.ExecuteAsync(q, new { code = o.Code, desc = o.Desc, reff = o.Ref, modifiedby = o.ModifiedBy, id = o.Id });
            }

            catch (Exception ex)
            {
                logger.LogError(ex, $"Error updating {table} with id {o.Id}");
            }
        }

        public async Task DeleteByIdAsync(long id, int userid, string table)
        {
            try
            {
                using var conn = ctx.CreateConnection();
                var q = $@"update {table} set deleted = true, deleted_by = @userid, deleted_date = now() where id = @id";
                await conn.ExecuteAsync(q, new { userid, id });
            }

            catch (Exception ex)
            {
                logger.LogError(ex, $"Error deleting {table} with id {id}");
            }
        }
    }
}
