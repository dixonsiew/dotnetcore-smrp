using Dapper;
using System.Data;
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
        public required string Password { get; set; }
        [JsonPropertyName("username")]
        public required string Username { get; set; }
        [JsonPropertyName("last_login")]
        public required DateTime LastLogin { get; set; }
        public List<Role>? Roles { get; set; }

        public static IEnumerable<User> GetQ(IEnumerable<dynamic> q, IDbConnection con)
        {
            return q.Select(o => new User {
                Id = o.id,
                FirstName = o.first_name,
                LastName = o.last_name,
                Password = o.password,
                Username = o.username,
                LastLogin = o.last_login,
                Roles = GetRoles(o.id, con)
            });
        }

        private async static Task<List<Role>> GetRoles(long id, IDbConnection con)
        {
            List<Role> lx = new List<Role>();
            var q = await con.QueryAsync(@"select aur.app_user_id, aur.roles_id, r.id, r.name from app_user_roles aur inner join role r on aur.roles_id = r.id where aur.app_user_id = @userId", new { userId = id});
            lx = Role.GetQ(q).ToList();
            return lx;
        }

        public static User FromQ(dynamic o)
        {
            return new User
            {
                Id = o.id,
                FirstName = o.first_name,
                LastName = o.last_name,
                Password = o.password,
                Username = o.username,
                LastLogin = o.last_login,
            };
        }
    }
}
