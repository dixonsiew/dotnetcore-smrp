using System.Text.Json.Serialization;

namespace smrp.Models
{
    public class Role
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }
        [JsonPropertyName("name")]
        public required string Name { get; set; }

        public static IEnumerable<Role> GetQ(IEnumerable<dynamic> q)
        {
            return q.Select(o => new Role
            {
                Id = o.id,
                Name = o.name,
            });
        }

        public static Role FromQ(dynamic o)
        {
            return new Role
            {
                Id = o.id,
                Name = o.name,
            };
        }
    }
}
