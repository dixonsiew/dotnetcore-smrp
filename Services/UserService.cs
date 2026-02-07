using Dapper;
using smrp.Models;
using System.Data;

namespace smrp.Services
{
    public class UserService
    {
        private readonly IDbConnection conn;

        public UserService(IDbConnection con)
        {
            conn = con;
        }

        public async Task<User?> FindByIdAsync(long id)
        {
            User? user = null;
            var q = await conn.QuerySingleOrDefaultAsync(@"select id, username, first_name, last_name, password, last_login from app_user where id = @id limit 1", new { id = id });
            if (q != null)
            {
                user = await User.SingleAsync(q, conn);
            }

            return user;
        }

        public async Task<User?> FindByUsernameAsync(string username)
        {
            User? user = null;
            var q = await conn.QuerySingleOrDefaultAsync(@"select id, username, first_name, last_name, password, last_login from app_user where username = @username limit 1", new { username = username });
            if (q != null)
            {
                user = await User.SingleAsync(q, conn);
            }

            return user;
        }

        public async Task<List<User>> FindAllAsync(int offset, int limit, string sortBy, string sortDir)
        {
            List<User> lx = new List<User>();
            var q = await conn.QueryAsync(@$"select t.id, t.username, t.first_name, t.last_name, t.password, t.last_login from app_user t order by {sortBy} {sortDir} offset @offset limit @limit", new { offset = offset, limit = limit });
            lx = await User.ListAsync(q, conn);
            return lx;
        }

        public async Task<int> CountAsync()
        {
            var q = await conn.ExecuteScalarAsync<int>(@"select count(id) from app_user");
            return q;
        }

        public async Task<bool> ExistsByOtherUsernameAsync(string username, long id)
        {
            var q = await conn.ExecuteScalarAsync<bool>(@"select exists (select 1 from app_user t where t.username = @username and t.id <> @id)", new { username = username, id = id });
            return q;
        }

        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            var q = await conn.ExecuteScalarAsync<bool>(@"select exists (select 1 from app_user t where t.username = @username)", new { username = username });
            return q;
        }

        public async Task<List<User>> FindByKeywordAsync(string keyword, int offset, int limit, string sortBy, string sortDir)
        {
            List<User> lx = new List<User>();
            var q = await conn.QueryAsync(@$"select t.id, t.username, t.first_name, t.last_name, t.password, t.last_login from app_user t where (t.username ilike @keyword or t.first_name ilike @keyword or t.last_name ilike @keyword) order by {sortBy} {sortDir} offset @offset limit @limit", new { keyword = keyword, offset = offset, limit = limit });
            lx = await User.ListAsync(q, conn);
            return lx;
        }

        public async Task<int> CountByKeywordAsync(string keyword)
        {
            var q = await conn.ExecuteScalarAsync<int>(@"select count(id) from app_user t where (t.username ilike @keyword or t.first_name ilike @keyword or t.last_name ilike @keyword)", new { keyword = keyword });
            return q;
        }

        public async Task UpdateLastLoginAsync(long id)
        {
            await conn.ExecuteAsync(@"update app_user set last_login = now() where id = @id", new { id = id });
        }

        public async Task UpdatePasswordAsync(User o)
        {
            string pw = BCrypt.Net.BCrypt.HashPassword(o.Password);
            await conn.ExecuteAsync(@"update app_user set password = @password where id = @id", new { password = pw, id = o.Id });
        }

        public bool ValidateCredentials(User user, string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, user.Password);
        }
    }
}
