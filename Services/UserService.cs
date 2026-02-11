using Dapper;
using smrp.Models;
using smrp.Utils;
using System.Data;
using System.Transactions;

namespace smrp.Services
{
    public class UserService
    {
        private readonly DefaultConnection ctx;

        public UserService(DefaultConnection c)
        {
            ctx = c;
        }

        public async Task<User?> FindByIdAsync(long id)
        {
            User? user = null;
            using var conn = ctx.CreateConnection();
            conn.Open();
            var q = await conn.QuerySingleOrDefaultAsync(@"select id, username, first_name, last_name, password, last_login from app_user where id = @id limit 1", new { id });
            if (q != null)
            {
                user = await User.SingleAsync(q, conn);
            }

            return user;
        }

        public async Task<User?> FindByUsernameAsync(string username)
        {
            User? user = null;
            using var conn = ctx.CreateConnection();
            conn.Open();
            var q = await conn.QuerySingleOrDefaultAsync(@"select id, username, first_name, last_name, password, last_login from app_user where username = @username limit 1", new { username });
            if (q != null)
            {
                user = await User.SingleAsync(q, conn);
            }

            return user;
        }

        public async Task<List<User>> FindAllAsync(int offset, int limit, string sortBy, string sortDir)
        {
            List<User> lx = new List<User>();
            using var conn = ctx.CreateConnection();
            conn.Open();
            var q = await conn.QueryAsync(@$"select t.id, t.username, t.first_name, t.last_name, t.password, t.last_login from app_user t order by {sortBy} {sortDir} offset @offset limit @limit", new { offset, limit });
            lx = User.List(q, conn);
            
            return lx;
        }

        public async Task<int> CountAsync()
        {
            using var conn = ctx.CreateConnection();
            conn.Open();
            int q = await conn.ExecuteScalarAsync<int>(@"select count(id) from app_user");
                
            return q;
        }

        public async Task<bool> ExistsByOtherUsernameAsync(string username, long id)
        {
            using var conn = ctx.CreateConnection();
            conn.Open();
            bool q = await conn.ExecuteScalarAsync<bool>(@"select exists (select 1 from app_user t where t.username = @username and t.id <> @id)", new { username, id });

            return q;
        }

        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            using var conn = ctx.CreateConnection();
            conn.Open();
            bool q = await conn.ExecuteScalarAsync<bool>(@"select exists (select 1 from app_user t where t.username = @username)", new { username });

            return q;
        }

        public async Task<List<User>> FindByKeywordAsync(string keyword, int offset, int limit, string sortBy, string sortDir)
        {
            List<User> lx = new List<User>();
            using var conn = ctx.CreateConnection();
            conn.Open();
            var q = await conn.QueryAsync(@$"select t.id, t.username, t.first_name, t.last_name, t.password, t.last_login from app_user t where (t.username ilike @keyword or t.first_name ilike @keyword or t.last_name ilike @keyword) order by {sortBy} {sortDir} offset @offset limit @limit", new { keyword = keyword, offset = offset, limit = limit });
            lx = await User.ListAsync(q, conn);

            return lx;
        }

        public async Task<int> CountByKeywordAsync(string keyword)
        {
            using var conn = ctx.CreateConnection();
            conn.Open();
            int q = await conn.ExecuteScalarAsync<int>(@"select count(id) from app_user t where (t.username ilike @keyword or t.first_name ilike @keyword or t.last_name ilike @keyword)", new { keyword = keyword });

            return q;
        }

        public async Task SaveAsync(User o)
        {
            string pwd = BCrypt.Net.BCrypt.HashPassword(o.Password);
            using var conn = ctx.CreateConnection();
            conn.Open();
            using (var scope = new TransactionScope())
            {
                var q = @"insert into app_user (id, username, password, first_name, last_name, active) values(nextval('app_user_id_seq'),@username,@pw,@firstName,@lastName,@active) returning id as app_user_id";
                var id = await conn.ExecuteScalarAsync<long>(q, new { username = o.Username, pw = pwd, firstName = o.FirstName, lastName = o.LastName, active = true });

                foreach (var r in o.Roles ?? [])
                {
                    var qr = @"insert into app_user_roles (app_user_id, roles_id) values(@userid, @roleid)";
                    await conn.ExecuteAsync(qr, new { userid = id, roleid = r.Id });
                }

                scope.Complete();
            }
        }

        public async Task UpdateAsync(User o)
        {
            using var conn = ctx.CreateConnection();
            conn.Open();
            using (var scope = new TransactionScope())
            {
                if (o.Password != "")
                {
                    string pwd = BCrypt.Net.BCrypt.HashPassword(o.Password);
                    var q = @"update app_user set password = @pw, first_name = @firstName, last_name = @lastName where id = @id";
                    await conn.ExecuteAsync(q, new { pw = pwd, firstName = o.FirstName, lastName = o.LastName, id = o.Id });
                }

                else
                {
                    var q = @"update app_user set first_name = @firstName, last_name = @lastName where id = @id";
                    await conn.ExecuteAsync(q, new { firstName = o.FirstName, lastName = o.LastName, id = o.Id });
                }

                var qd = @"delete from app_user_roles where app_user_id = @id";
                await conn.ExecuteAsync(qd, new { id = o.Id });

                foreach (var r in o.Roles ?? [])
                {
                    var qr = @"insert into app_user_roles (app_user_id, roles_id) values(@userid, @roleid)";
                    await conn.ExecuteAsync(qr, new { userid = o.Id, roleid = r.Id });
                }

                scope.Complete();
            }
        }

        public async Task DeleteByIdAsync(long id)
        {
            using var conn = ctx.CreateConnection();
            conn.Open();
            using (var scope = new TransactionScope())
            {
                var q = @"delete from app_user_roles where app_user_id = @id";
                await conn.ExecuteAsync(q, new { id });

                q = @"delete from app_user where id = @id";
                await conn.ExecuteAsync(q, new { id });

                scope.Complete();
            }
        }

        public async Task UpdateLastLoginAsync(long id)
        {
            using var conn = ctx.CreateConnection();
            conn.Open();
            await conn.ExecuteAsync(@"update app_user set last_login = now() where id = @id", new { id });
        }

        public async Task UpdatePasswordAsync(User o)
        {
            string pw = BCrypt.Net.BCrypt.HashPassword(o.Password);
            using var conn = ctx.CreateConnection();
            conn.Open();
            await conn.ExecuteAsync(@"update app_user set password = @password where id = @id", new { password = pw, id = o.Id });
        }

        public bool ValidateCredentials(User user, string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, user.Password);
        }
    }
}
