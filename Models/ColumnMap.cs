using System.Text.Json.Serialization;

namespace smrp.Models
{
    public class ColumnMap
    {
        [JsonPropertyName("field")]
        public string Field { get; set; }
        [JsonPropertyName("text")]
        public string Text { get; set; }

        public ColumnMap(string field, string text)
        {
            Field = field;
            Text = text;
        }
    }
}
