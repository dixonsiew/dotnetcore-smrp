using Dapper;
using smrp.Controllers;
using smrp.Models;
using smrp.Utils;
using System.Data;
using System.Transactions;

namespace smrp.Services
{
    public class UserService
    {
        private readonly DefaultConnection ctx;
        private readonly ILogger logger;

        public UserService(DefaultConnection c, ILogger log)
        {
            ctx = c;
            logger = log;
        }

        public async Task<User?> FindByIdAsync(long id)
        {
            User? user = null;
            try
            {
                using var conn = ctx.CreateConnection();
                var q = await conn.QuerySingleOrDefaultAsync(@"select id, username, first_name, last_name, password, last_login from app_user where id = @id limit 1", new { id });
                if (q != null)
                {
                    user = await User.SingleAsync(q, conn);
                }
            }
            
            catch (Exception ex)
            {
                logger.LogError(ex, "Error finding user by id: {Id}", id);
            }

            return user;
        }

        public async Task<User?> FindByUsernameAsync(string username)
        {
            User? user = null;
            try
            {
                using var conn = ctx.CreateConnection();
                var q = await conn.QuerySingleOrDefaultAsync(@"select id, username, first_name, last_name, password, last_login from app_user where username = @username limit 1", new { username });
                if (q != null)
                {
                    user = await User.SingleAsync(q, conn);
                }
            }
            
            catch (Exception ex)
            {
                logger.LogError(ex, "Error finding user by username: {Username}", username);
            }

            return user;
        }

        public async Task<List<User>> FindAllAsync(int offset, int limit, string sortBy, string sortDir)
        {
            List<User> lx = new List<User>();
            try
            {
                using var conn = ctx.CreateConnection();
                var q = await conn.QueryAsync(@$"select t.id, t.username, t.first_name, t.last_name, t.password, t.last_login from app_user t order by {sortBy} {sortDir} offset @offset limit @limit", new { offset, limit });
                lx = User.List(q, conn);

                return lx;
            }
            
            catch (Exception ex)
            {
                logger.LogError(ex, "Error finding all users with offset: {Offset}, limit: {Limit}, sortBy: {SortBy}, sortDir: {SortDir}", offset, limit, sortBy, sortDir);
            }

            return lx;
        }

        public async Task<int> CountAsync()
        {
            int q = 0;
            try
            {
                using var conn = ctx.CreateConnection();
                q = await conn.ExecuteScalarAsync<int>(@"select count(id) from app_user");
            }
            
            catch (Exception ex)
            {
                logger.LogError(ex, "Error counting users");
            }

            return q;
        }

        public async Task<bool> ExistsByOtherUsernameAsync(string username, long id)
        {
            bool q = false;
            try
            {
                using var conn = ctx.CreateConnection();
                q = await conn.ExecuteScalarAsync<bool>(@"select exists (select 1 from app_user t where t.username = @username and t.id <> @id)", new { username, id });
            }
            
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking if other user exists with username: {Username} and id: {Id}", username, id);
            }

            return q;
        }

        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            bool q = false;
            try
            {
                using var conn = ctx.CreateConnection();
                q = await conn.ExecuteScalarAsync<bool>(@"select exists (select 1 from app_user t where t.username = @username)", new { username });
            }
            
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking if user exists with username: {Username}", username);
            }

            return q;
        }

        public async Task<List<User>> FindByKeywordAsync(string keyword, int offset, int limit, string sortBy, string sortDir)
        {
            List<User> lx = new List<User>();
            try
            {
                using var conn = ctx.CreateConnection();
                var q = await conn.QueryAsync(@$"select t.id, t.username, t.first_name, t.last_name, t.password, t.last_login from app_user t where (t.username ilike @keyword or t.first_name ilike @keyword or t.last_name ilike @keyword) order by {sortBy} {sortDir} offset @offset limit @limit", new { keyword, offset, limit });
                lx = await User.ListAsync(q, conn);
            }

            catch (Exception ex)
            {
                logger.LogError(ex, "Error finding users by keyword: {Keyword} with offset: {Offset}, limit: {Limit}, sortBy: {SortBy}, sortDir: {SortDir}", keyword, offset, limit, sortBy, sortDir);
            }

            return lx;
        }

        public async Task<int> CountByKeywordAsync(string keyword)
        {
            int q = 0;
            try
            {
                using var conn = ctx.CreateConnection();
                q = await conn.ExecuteScalarAsync<int>(@"select count(id) from app_user t where (t.username ilike @keyword or t.first_name ilike @keyword or t.last_name ilike @keyword)", new { keyword });
            }

            catch (Exception ex)
            {
                logger.LogError(ex, "Error counting users by keyword: {Keyword}", keyword);
            }

            return q;
        }

        public async Task SaveAsync(User o)
        {
            try
            {
                string pwd = BCrypt.Net.BCrypt.HashPassword(o.Password);
                using var conn = ctx.CreateConnection();
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

            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving user with username: {Username}", o.Username);
            }
        }

        public async Task UpdateAsync(User o)
        {
            try
            {
                using var conn = ctx.CreateConnection();
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

            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating user with id: {Id}", o.Id);
            }
        }

        public async Task DeleteByIdAsync(long id)
        {
            try
            {
                using var conn = ctx.CreateConnection();
                using (var scope = new TransactionScope())
                {
                    var q = @"delete from app_user_roles where app_user_id = @id";
                    await conn.ExecuteAsync(q, new { id });

                    q = @"delete from app_user where id = @id";
                    await conn.ExecuteAsync(q, new { id });

                    scope.Complete();
                }
            }

            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting user with id: {Id}", id);
            }
        }

        public async Task UpdateLastLoginAsync(long id)
        {
            try
            {
                using var conn = ctx.CreateConnection();
                await conn.ExecuteAsync(@"update app_user set last_login = now() where id = @id", new { id });
            }
            
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating last login for user with id: {Id}", id);
            }
        }

        public async Task UpdatePasswordAsync(User o)
        {
            try
            {
                string pw = BCrypt.Net.BCrypt.HashPassword(o.Password);
                using var conn = ctx.CreateConnection();
                await conn.ExecuteAsync(@"update app_user set password = @password where id = @id", new { password = pw, id = o.Id });
            }
            
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating password for user with id: {Id}", o.Id);
            }
        }

        public bool ValidateCredentials(User user, string password)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, user.Password);
            }
            
            catch (Exception ex)
            {
                logger.LogError(ex, "Error validating credentials for user with id: {Id}", user.Id);
            }

            return false;
        }
    }
}
