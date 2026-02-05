using System.Text.Json.Serialization;

namespace smrp.Models
{
    public class AppUser
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }
        [JsonPropertyName("active")]
        public bool Active { get; set; }
        [JsonPropertyName("first_name")]
        public required string FirstName { get; set; }
        [JsonPropertyName("last_name")]
        public required string LastName { get; set; }
        [JsonPropertyName("password")]
        public required string Password { get; set; }
        [JsonPropertyName("username")]
        public required string Username { get; set; }
        [JsonPropertyName("last_login")]
        public required DateTime LastLogin { get; set; }

        public static IEnumerable<AppUser> GetQ(IEnumerable<dynamic> q)
        {
            return q.Select(o => new AppUser
            {
                Id = o.id,
                Active = o.active,
                FirstName = o.first_name,
                LastName = o.last_name,
                Password = o.password,
                Username = o.username,
                LastLogin = o.last_login,
            });
        }
    }
}
