using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace smrp.Dtos
{
    public class UserDto
    {
        [Required]
        [StringLength(maximumLength: 150)]
        public required string Username { get; set; }

        [Required]
        [StringLength(maximumLength: 150)]
        public required string Password { get; set; }

        [JsonPropertyName("first_name")]
        [Required]
        [StringLength(maximumLength: 150)]
        public required string Firstname { get; set; }

        [JsonPropertyName("last_name")]
        [StringLength(maximumLength: 150)]
        public string? Lastname { get; set; }

        [JsonPropertyName("role_id")]
        public required long RoleId { get; set; }
    }
}
