using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContractClaimSystem.Models
{
    public class Claim
    {
        [Key]
        public int ClaimId { get; set; }

        public string LecturerId { get; set; } = string.Empty;

        public decimal HoursWorked { get; set; }

        public decimal HourlyRate { get; set; }
        [NotMapped]
        public decimal TotalAmount => HoursWorked * HourlyRate;



        public string AdditionalNotes { get; set; } = string.Empty;

        public DateTime SubmissionDate { get; set; }

        public ClaimStatus Status { get; set; } = ClaimStatus.Pending;

        public DateTime? ReviewDate { get; set; }

        public string? ReviewerId { get; set; }

        // Navigation
        public virtual User Lecturer { get; set; } = null!;

        public virtual ICollection<SupportingDocument> SupportingDocuments { get; set; } = new List<SupportingDocument>();
    }
}
