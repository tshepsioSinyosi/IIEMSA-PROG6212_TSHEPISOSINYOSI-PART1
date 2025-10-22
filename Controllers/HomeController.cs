using ContractClaimSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContractClaimSystem.Data;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace ContractClaimSystem.Controllers
{
    [Authorize] // Enforce authentication for all home actions
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger,
                              UserManager<User> userManager,
                              SignInManager<User> signInManager,
                              ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // Action run after successful login to determine redirection
        public async Task<IActionResult> Index()
        {
            if (User.Identity!.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    await _signInManager.SignOutAsync();
                    return RedirectToAction("Login", "Account");
                }

                // Redirect based on role
                if (await _userManager.IsInRoleAsync(user, "Coordinator"))
                    return RedirectToAction("CoordinatorDashboard");
                if (await _userManager.IsInRoleAsync(user, "Manager"))
                    return RedirectToAction("ManagerDashboard");
                if (await _userManager.IsInRoleAsync(user, "Lecturer"))
                    return RedirectToAction("LecturerDashboard");
            }

            return RedirectToAction("Login", "Account");
        }

        // Lecturer Dashboard: shows their own claims
        [Authorize(Roles = "Lecturer")]
        public async Task<IActionResult> LecturerDashboard()
        {
            var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (lecturerId == null) return Unauthorized();

            var claims = await _context.Claims
                .Include(c => c.Lecturer)
                .Include(c => c.SupportingDocuments)
                .Where(c => c.LecturerId == lecturerId)
                .OrderByDescending(c => c.SubmissionDate)
                .ToListAsync();

            return View(claims);
        }

        // Coordinator Dashboard: shows all pending claims for approval
        [Authorize(Roles = "Coordinator")]
        public async Task<IActionResult> CoordinatorDashboard()
        {
            var pendingClaims = await _context.Claims
                .Include(c => c.Lecturer)
                .Include(c => c.SupportingDocuments)
                .Where(c => c.Status == ClaimStatus.Pending)
                .OrderBy(c => c.SubmissionDate)
                .ToListAsync();

            return View(pendingClaims);
        }

        // Manager Dashboard: shows all claims (can add filters for summary later)
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> ManagerDashboard()
        {
            var allClaims = await _context.Claims
                .Include(c => c.Lecturer)
                .Include(c => c.SupportingDocuments)
                .OrderByDescending(c => c.SubmissionDate)
                .ToListAsync();

            return View(allClaims);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
