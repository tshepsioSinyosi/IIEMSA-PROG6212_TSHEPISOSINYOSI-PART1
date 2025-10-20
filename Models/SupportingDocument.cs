using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContractMonthlyClaimsSystem.Models
{
    public class SupportingDocument
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Claim")]
        public int ClaimId { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; }

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; }

        [Required]
        public DateTime UploadDate { get; set; }

        // Navigation property
        public virtual Claim Claim { get; set; }
    }
}
