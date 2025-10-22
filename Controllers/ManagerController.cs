using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ContractClaimSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace ContractMonthlyClaimsSystem.Controllers
{
    [Authorize(Roles = "Coordinator, Manager")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ManagerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var pendingClaims = await _context.Claims
                .Include(c => c.Lecturer)
                .Where(c => c.Status == ClaimStatus.Pending)
                .ToListAsync();

            return View(pendingClaims);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null) return NotFound();

            claim.Status = ClaimStatus.Approved;
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null) return NotFound();

            claim.Status = ClaimStatus.Rejected;
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard");
        }
    }
}
