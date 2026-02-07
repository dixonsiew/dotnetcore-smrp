using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace smrp.Dtos
{
    public class ChangePasswordDto
    {
        [Required]
        [StringLength(maximumLength: 150)]
        public required string Password { get; set; }

        [JsonPropertyName("confirm_password")]
        [Required]
        [StringLength(maximumLength: 150)]
        public required string ConfirmPassword { get; set; }
    }
}
