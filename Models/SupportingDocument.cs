using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContractClaimSystem.Models
{
    public class SupportingDocument
    {
        [Key]
        public int Id { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty;

        [ForeignKey("Claim")]
        public int ClaimId { get; set; }

        public virtual Claim Claim { get; set; } = null!;

        // Add UploadDate property
        public DateTime UploadDate { get; set; }
    }
}
