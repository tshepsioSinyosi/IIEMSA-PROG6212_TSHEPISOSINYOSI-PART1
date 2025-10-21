using ContractMonthlyClaimsSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContractClaimSystem.Data;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication; // Added this using directive, although SignInManager is usually sufficient

namespace ContractClaimSystem.Controllers
{
    [Authorize] // Enforce authentication for all home actions
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager; // Added SignInManager
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, UserManager<User> userManager, SignInManager<User> signInManager, ApplicationDbContext context) // Inject SignInManager
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager; // Initialize SignInManager
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
                    // FIX: Use _signInManager.SignOutAsync() instead of _userManager.SignOutAsync()
                    await _signInManager.SignOutAsync();
                    return RedirectToAction("Login", "Account");
                }

                // Check for Coordinator role first (highest priority)
                if (await _userManager.IsInRoleAsync(user, "Coordinator"))
                {
                    _logger.LogInformation($"User {user.Email} assigned Identity Roles: Coordinator");
                    return RedirectToAction("CoordinatorDashboard");
                }
                // Check for Manager role
                else if (await _userManager.IsInRoleAsync(user, "Manager"))
                {
                    _logger.LogInformation($"User {user.Email} assigned Identity Roles: Manager");
                    return RedirectToAction("ManagerDashboard");
                }
                // Default to Lecturer dashboard
                else if (await _userManager.IsInRoleAsync(user, "Lecturer"))
                {
                    _logger.LogInformation($"User {user.Email} assigned Identity Roles: Lecturer");
                    return RedirectToAction("LecturerDashboard");
                }
            }

            // Fallback for unauthenticated users
            return RedirectToAction("Login", "Account");
        }

        [Authorize(Roles = "Lecturer")]
        public async Task<IActionResult> LecturerDashboard()
        {
            // Get the ID of the currently logged-in Lecturer
            var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (lecturerId == null)
            {
                // Should not happen, but safe check
                return Unauthorized();
            }

            // Fetch all claims associated with this Lecturer, ordered by submission date
            var claims = await _context.Claims
                .Where(c => c.LecturerId == lecturerId)
                .OrderByDescending(c => c.SubmissionDate)
                .ToListAsync();

            // Pass the list of claims to the view for display
            return View(claims);
        }

        [Authorize(Roles = "Coordinator")]
        public IActionResult CoordinatorDashboard()
        {
            // This view will be updated in the next step to show claims for approval
            return View();
        }

        [Authorize(Roles = "Manager")]
        public IActionResult ManagerDashboard()
        {
            // This view will be updated later for global overview
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
