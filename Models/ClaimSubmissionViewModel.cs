using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ContractClaimSystem.Models
{
    public class ClaimSubmissionViewModel
    {
        [Required]
        public decimal HoursWorked { get; set; }

        [Required]
        public decimal HourlyRate { get; set; }

        [Required]
        [MaxLength(500)]
        public string Notes { get; set; } = string.Empty;

        public List<IFormFile>? SupportingFiles { get; set; }
    }
}
