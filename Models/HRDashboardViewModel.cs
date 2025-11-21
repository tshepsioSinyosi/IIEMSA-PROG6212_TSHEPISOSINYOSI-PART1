using System.Collections.Generic;

namespace ContractClaimSystem.Models
{
    public class HRDashboardViewModel
    {
        public List<Claim> Claims { get; set; } = new();
        public List<User> Lecturers { get; set; } = new();
        public int TotalApprovedClaims { get; set; }
        public decimal TotalPayment { get; set; }
    }
}
