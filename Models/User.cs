using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ContractMonthlyClaimsSystem.Models
{
    public class User : IdentityUser
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        // Example roles: "Lecturer", "Coordinator", "Manager"
        [MaxLength(50)]
        public string Role { get; set; }

        // Optional navigation property if you want to list claims submitted by the lecturer
        public virtual ICollection<Claim> Claims { get; set; }
    }
}
