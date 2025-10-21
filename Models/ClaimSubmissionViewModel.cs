// Models/ClaimSubmissionViewModel.cs
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;


namespace ContractMonthlyClaimsSystem.Models  // ✅ Must match exactly
{
    public class ClaimSubmissionViewModel
    {
        [Required]
        [Range(0.1, 1000, ErrorMessage = "Hours must be positive")]
        public double HoursWorked { get; set; }

        [Required]
        [Range(0.01, 10000, ErrorMessage = "Hourly rate must be positive")]
        public decimal HourlyRate { get; set; }

        public string Notes { get; set; }

        // Allow multiple supporting documents if you want
        [Display(Name = "Supporting Document")]
        public List<IFormFile> SupportingFiles { get; set; }
    }
}