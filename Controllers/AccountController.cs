using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using ContractMonthlyClaimsSystem.Models;
using ContractClaimSystem.Models;

namespace ContractClaimSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    ModelState.AddModelError("", "User not found.");
                    return View(model);
                }

                // Log roles for debugging
                var roles = await _userManager.GetRolesAsync(user);
                _logger.LogInformation($"User {user.UserName} logged in with roles: {string.Join(", ", roles)}");

                // Redirect based on role (exact casing!)
                if (await _userManager.IsInRoleAsync(user, "Coordinator"))
                    return RedirectToAction("CoordinatorDashboard", "Home");
                else if (await _userManager.IsInRoleAsync(user, "Lecturer"))
                    return RedirectToAction("LecturerDashboard", "Home");
                else if (await _userManager.IsInRoleAsync(user, "Manager"))
                    return RedirectToAction("ManagerDashboard", "Home");
                else
                    return LocalRedirect(returnUrl ?? Url.Action("Index", "Home"));
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError("", "Your account is locked out.");
            }
            else
            {
                _logger.LogWarning($"Invalid login attempt for {model.Email}");
                ModelState.AddModelError("", "Invalid email or password.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
