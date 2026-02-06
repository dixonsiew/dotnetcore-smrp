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

        public async Task<User?> FindById(int id)
        {
            User? user = null;
            var q = await conn.QuerySingleOrDefaultAsync(@"select id, username, first_name, last_name, password, last_login from app_user where id = @id limit 1", new { id = id });
            if (q != null)
            {
                user = User.FromQ(q);
            }

            return user;
        }

        public async Task<User?> FindByUsername(string username)
        {
            User? user = null;
            var q = await conn.QuerySingleOrDefaultAsync(@"select id, username, first_name, last_name, password, last_login from app_user where username = @username limit 1", new { username = username });
            if (q != null)
            {
                user = User.FromQ(q);
            }

            return user;
        }

        public async Task<List<User>> FindAllAsync(int offset, int limit, string sortBy, string sortDir)
        {
            List<User> lx = new List<User>();
            var q = await conn.QueryAsync(@$"select t.id, t.username, t.first_name, t.last_name, t.password, t.last_login from app_user t order by {sortBy} {sortDir} offset @offset limit @limit", new { offset = offset, limit = limit });
            lx = User.GetQ(q, conn).ToList();
            return lx;
        }

        public async Task<long> Count()
        {
            var q = await conn.ExecuteScalarAsync<long>(@"select count(id) from app_user");
            return q;
        }

        public async Task<bool> ExistsByOtherUsername(string username, int id)
        {
            var q = await conn.ExecuteScalarAsync<bool>(@"select exists (select 1 from app_user t where t.username = @username and t.id <> @id)", new { username = username, id = id });
            return q;
        }

        public async Task<bool> ExistsByUsername(string username, int id)
        {
            var q = await conn.ExecuteScalarAsync<bool>(@"select exists (select 1 from app_user t where t.username = @username)", new { username = username });
            return q;
        }

        public bool ValidateCredentials(User user, string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, user.Password);
        }
    }
}
