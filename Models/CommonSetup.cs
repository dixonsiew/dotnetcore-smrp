using System.Data;
using System.Text.Json.Serialization;

namespace smrp.Models
{
    public class CommonSetup
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        public required string Code { get; set; }

        public required string Desc { get; set; }

        public required string Ref { get; set; }

        [JsonPropertyName("created_by")]
        public int CreatedBy { get; set; }

        [JsonPropertyName("CreatedDate")]
        public string? CreatedDate { get; set; }

        [JsonPropertyName("modified_by")]
        public int? ModifiedBy { get; set; }

        [JsonPropertyName("modified_date")]
        public string? ModifiedDate { get; set; }

        [JsonPropertyName("deleted")]
        public required bool Deleted { get; set; }

        [JsonPropertyName("deleted_by")]
        public int? DeletedBy { get; set; }

        [JsonPropertyName("deleted_date")]
        public string? DeletedDate { get; set; }

        public static List<CommonSetup> List(IEnumerable<dynamic> q)
        {
            return q.Select(o => new CommonSetup
            {
                Id = o.id,
                Code = o.code,
                Desc = o.desc,
                Ref = o.reff,
                CreatedBy = o.created_by,
                CreatedDate = o.created_date,
                ModifiedBy = o.modified_by,
                ModifiedDate = o.modified_date,
                Deleted = o.deleted,
                DeletedBy = o.deleted_by,
                DeletedDate = o.deleted_date,
            }).ToList();
        }

        public static CommonSetup Single(dynamic o)
        {
            return new CommonSetup
            {
                Id = o.id,
                Code = o.code,
                Desc = o.desc,
                Ref = o.reff,
                CreatedBy = o.created_by,
                CreatedDate = o.created_date,
                ModifiedBy = o.modified_by,
                ModifiedDate = o.modified_date,
                Deleted = o.deleted,
                DeletedBy = o.deleted_by,
                DeletedDate = o.deleted_date,
            };
        }
    }
}
