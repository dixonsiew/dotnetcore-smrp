using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace smrp.Dtos
{
    public class ReportQueryDto
    {
        [JsonPropertyName("_page")]
        public int Page { get; set; }

        [JsonPropertyName("_limit")]
        public int Limit { get; set; }

        [JsonPropertyName("vt")]
        public int Vt { get; set; }

        [JsonPropertyName("datefrom")]
        [Required]
        public required string DateFrom { get; set; }

        [JsonPropertyName("dateto")]
        [Required]
        public required string DateTo { get; set; }
    }
}
