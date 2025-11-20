using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ContractClaimSystem.Models
{
    public class ClaimSubmissionViewModel
    {
        [Required]
        [Range(1, 300, ErrorMessage = "Hours worked must be between 1 and 300.")]
        public decimal HoursWorked { get; set; }

        [Required]
        [Range(50, 2000, ErrorMessage = "Hourly rate must be between R50 and R2000.")]
        public decimal HourlyRate { get; set; }

        [Required]
        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        public List<IFormFile>? SupportingFiles { get; set; }

        // Auto-calculated amount (read-only)
        public decimal TotalAmount => HoursWorked * HourlyRate;
    }
}
