// Models/Claim.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ContractMonthlyClaimsSystem.Models;

public class Claim
{
    [Key]
    public int Id { get; set; }
    public User Lecturer { get; set; }
    

    [Required]
    public string LecturerId { get; set; } // FK to Identity User (User.Id)

    [Required]
    public double HoursWorked { get; set; }

    [Required]
    public decimal HourlyRate { get; set; }

    public string Notes { get; set; }

    public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;

    public ClaimStatus Status { get; set; } = ClaimStatus.Pending;

    // Navigation
    public ICollection<SupportingDocument> SupportingDocuments { get; set; }
}
