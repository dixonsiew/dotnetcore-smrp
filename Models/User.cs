using Dapper;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;

namespace smrp.Models
{
    public class User
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("first_name")]
        public required string FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public required string LastName { get; set; }

        [JsonIgnore]
        public string? Password { get; set; }

        [JsonPropertyName("username")]
        public required string Username { get; set; }

        [JsonPropertyName("last_login")]
        public string? LastLogin { get; set; }

        public List<Role>? Roles { get; set; }

        public static List<User> List(IEnumerable<dynamic> q, IDbConnection con)
        {
            return q.Select(o => new User
            {
                Id = o.id,
                FirstName = o.first_name,
                LastName = o.last_name,
                Password = o.password,
                Username = o.username,
                LastLogin = o.last_login?.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Roles = GetRoles(o.id, con),
            }).ToList();
        }

        public static async Task<List<User>> ListAsync(IEnumerable<dynamic> q, IDbConnection con)
        {
            var qx = q.ToAsyncEnumerable();
            var qs = qx.Select(async o => new User
            {
                Id = o.id,
                FirstName = o.first_name,
                LastName = o.last_name,
                Password = o.password,
                Username = o.username,
                LastLogin = o.last_login?.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Roles = await GetRolesAsync(o.id, con),
            });
            var rx = await qs.ToListAsync();
            List<User> lx = new List<User>();
            foreach (var x in rx)
            {
                lx.Add(await x);
            }
            return lx;
        }

        private static List<Role> GetRoles(long id, IDbConnection con)
        {
            List<Role> lx = new List<Role>();
            var q = con.Query(@"select aur.app_user_id, aur.roles_id, r.id, r.name from app_user_roles aur inner join role r on aur.roles_id = r.id where aur.app_user_id = @userId", new { userId = id });
            lx = Role.List(q);
            return lx;
        }

        private static async Task<List<Role>> GetRolesAsync(long id, IDbConnection con)
        {
            List<Role> lx = new List<Role>();
            var q = await con.QueryAsync(@"select aur.app_user_id, aur.roles_id, r.id, r.name from app_user_roles aur inner join role r on aur.roles_id = r.id where aur.app_user_id = @userId", new { userId = id });
            lx = Role.List(q);
            return lx;
        }

        public static async Task<User> SingleAsync(dynamic o, IDbConnection con)
        {
            return new User
            {
                Id = o.id,
                FirstName = o.first_name,
                LastName = o.last_name,
                Password = o.password,
                Username = o.username,
                LastLogin = o.last_login?.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Roles = await GetRolesAsync(o.id, con),
            };
        }
    }
}
