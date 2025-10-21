using System.Diagnostics;
using ContractClaimSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContractClaimSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        // --- NEW DASHBOARD ACTIONS ADDED BELOW ---

        // Action for the Coordinator Dashboard
        // Only users with the "Coordinator" role can access this.
        [Authorize(Roles = "Coordinator")]
        public IActionResult CoordinatorDashboard()
        {
            ViewData["Title"] = "Coordinator Dashboard";
            return View();
        }

        // Action for the Lecturer Dashboard
        // Only users with the "Lecturer" role can access this.
        [Authorize(Roles = "Lecturer")]
        public IActionResult LecturerDashboard()
        {
            ViewData["Title"] = "Lecturer Dashboard";
            return View();
        }
        
        [Authorize(Roles = "Manager")]
        public IActionResult ManagerDashboard()
        {
            ViewData["Title"] = "Manager Dashboard";
            return View();
        }

        // --- END OF NEW DASHBOARD ACTIONS ---

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
