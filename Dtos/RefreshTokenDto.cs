using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace smrp.Dtos
{
    public class RefreshTokenDto
    {
        [Required]
        [JsonPropertyName("refresh_token")]
        public required string RefreshToken { get; set; }
    }
}
