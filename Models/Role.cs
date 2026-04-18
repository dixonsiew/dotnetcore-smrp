using System.Text.Json.Serialization;

namespace smrp.Models
{
    public class Role
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        public static List<Role> List(IEnumerable<dynamic> q)
        {
            return q.Select(FromRs).ToList();
        }

        public static Role FromRs(dynamic o)
        {
            return new Role
            {
                Id = o.id,
                Name = o.name,
            };
        }
    }
}
