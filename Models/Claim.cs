using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ContractClaimSystem.Models;

namespace ContractMonthlyClaimsSystem.Models
{
    public enum ClaimStatus
    {
        Pending,
        Approved,
        Rejected
    }

    public class Claim
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("Lecturer")]
        public string LecturerId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public double HoursWorked { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal HourlyRate { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; }

        [Required]
        public DateTime SubmissionDate { get; set; }

        [Required]
        public ClaimStatus Status { get; set; } = ClaimStatus.Pending;

        // Navigation property
        public virtual User Lecturer { get; set; }

        // Navigation property for supporting documents
        public virtual ICollection<SupportingDocument> SupportingDocuments { get; set; }
    }
}
