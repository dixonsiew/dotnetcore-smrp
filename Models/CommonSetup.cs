using System.Data;
using System.Globalization;
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
        public long CreatedBy { get; set; }

        [JsonPropertyName("CreatedDate")]
        public string? CreatedDate { get; set; }

        [JsonPropertyName("modified_by")]
        public long? ModifiedBy { get; set; }

        [JsonPropertyName("modified_date")]
        public string? ModifiedDate { get; set; }

        [JsonPropertyName("deleted")]
        public bool? Deleted { get; set; }

        [JsonPropertyName("deleted_by")]
        public long? DeletedBy { get; set; }

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
                CreatedDate = o.created_date?.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                ModifiedBy = o.modified_by,
                ModifiedDate = o.modified_date?.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Deleted = o.deleted,
                DeletedBy = o.deleted_by,
                DeletedDate = o.deleted_date?.ToString("yyyy-MM-ddTHH:mm:ssZ"),
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
                CreatedDate = o.created_date?.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                ModifiedBy = o.modified_by,
                ModifiedDate = o.modified_date?.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                Deleted = o.deleted,
                DeletedBy = o.deleted_by,
                DeletedDate = o.deleted_date?.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            };
        }
    }
}
