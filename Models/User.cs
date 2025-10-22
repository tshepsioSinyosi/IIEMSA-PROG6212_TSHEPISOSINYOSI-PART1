using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ContractClaimSystem.Models
{
    public class User : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Role { get; set; } = string.Empty;

        public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();
    }
}
