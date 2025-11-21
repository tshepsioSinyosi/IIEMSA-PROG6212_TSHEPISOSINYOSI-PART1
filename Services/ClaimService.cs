using ContractClaimSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContractClaimSystem.Services
{
    public class ClaimService
    {
        private readonly ApplicationDbContext _context;

        public ClaimService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get claims optionally filtered by status
        public async Task<List<Claim>> GetClaimsAsync(ClaimStatus? status = null)
        {
            var query = _context.Claims.AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
            }

            return await query.ToListAsync();
        }

        // Approve multiple claims
        public async Task ApproveClaimsAsync(List<int> claimIds)
        {
            var claims = await _context.Claims.Where(c => claimIds.Contains(c.ClaimId)).ToListAsync();

            foreach (var claim in claims)
                claim.Status = ClaimStatus.Approved;

            await _context.SaveChangesAsync();
        }

        // Generate report (PDF/Excel)
        public async Task<byte[]> GenerateInvoiceReportAsync()
        {
            var approvedClaims = await _context.Claims
                .Where(c => c.Status == ClaimStatus.Approved)
                .ToListAsync();

            var reportData = approvedClaims
                .GroupBy(c => c.LecturerId)
                .Select(g => new
                {
                    LecturerId = g.Key,
                    TotalHours = g.Sum(c => c.HoursWorked),
                    TotalPayment = g.Sum(c => c.HoursWorked * c.HourlyRate)
                })
                .ToList();

            // TODO: generate PDF/Excel (iTextSharp, EPPlus)
            return new byte[0];
        }
    }
}
