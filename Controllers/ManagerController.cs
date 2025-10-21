using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContractMonthlyClaimsSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace ContractClaimSystem.Controllers
{
    // Authorize Manager to access the dashboard
    [Authorize(Roles = "Manager")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public ManagerController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Manager/Dashboard
        // Shows all claims currently awaiting Manager approval (Pending status)
        public async Task<IActionResult> Dashboard()
        {
            // Fetch all claims that are currently marked as "Pending"
            // We include the Lecturer (User) data so we can display their name in the view
            var pendingClaims = _context.Claims
    .Where(c => c.Status == ClaimStatus.Pending)
    .ToList();


            return View(pendingClaims);
        }
    }
}
