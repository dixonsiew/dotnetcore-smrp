using System.ComponentModel.DataAnnotations;

namespace smrp.Dtos
{
    public class CommonSetupDto
    {
        [Required]
        [StringLength(maximumLength: 30)]
        public required string Code { get; set; }

        [Required]
        [StringLength(maximumLength: 300)]
        public required string Desc { get; set; }

        [Required]
        [StringLength(maximumLength: 200)]
        public required string Ref { get; set; }
    }
}
