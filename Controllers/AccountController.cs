using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
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

            if (!ModelState.IsValid) return View(model);

            try
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user == null)
                    {
                        ModelState.AddModelError("", "User not found.");
                        return View(model);
                    }

                    var roles = await _userManager.GetRolesAsync(user);
                    var users = _userManager.Users.ToList();
                    _logger.LogInformation($"Users in DB: {string.Join(", ", users.Select(u => u.Email))}");
                    _logger.LogInformation($"Login result: {result.Succeeded}, LockedOut: {result.IsLockedOut}, RequiresTwoFactor: {result.RequiresTwoFactor}");

                    if (await _userManager.IsInRoleAsync(user, "Coordinator"))
                        return RedirectToAction("CoordinatorDashboard", "Home");
                    else if (await _userManager.IsInRoleAsync(user, "Lecturer"))
                        return RedirectToAction("LecturerDashboard", "Home");
                    else if (await _userManager.IsInRoleAsync(user, "Manager"))
                        return RedirectToAction("ManagerDashboard", "Home");
                    else if (await _userManager.IsInRoleAsync(user, "HR"))
                        return RedirectToAction("Index", "HR"); // HR dashboard
                    else
                        return LocalRedirect(returnUrl ?? Url.Action("Index", "Home"));

                }

                if (result.IsLockedOut)
                    ModelState.AddModelError("", "Your account is locked out.");
                else
                {
                    _logger.LogWarning($"Invalid login attempt for {model.Email}");
                    ModelState.AddModelError("", "Invalid email or password.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during login for user {model.Email}");
                ModelState.AddModelError("", "An unexpected error occurred. Please try again later.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _signInManager.SignOutAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout.");
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var user = new User
                {
                    UserName = model.Email,
                    Email = model.Email
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Lecturer");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("LecturerDashboard", "Home");
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during registration for {model.Email}");
                ModelState.AddModelError("", "An unexpected error occurred during registration.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                    return RedirectToAction("ForgotPasswordConfirmation");

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetLink = Url.Action("ResetPassword", "Account", new { token, email = model.Email }, Request.Scheme);

                _logger.LogInformation($"Password reset link for {model.Email}: {resetLink}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating forgot password link for {model.Email}");
                ModelState.AddModelError("", "An unexpected error occurred. Please try again later.");
            }

            return RedirectToAction("ForgotPasswordConfirmation");
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation() => View();

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null) return BadRequest();
            return View(new ResetPasswordViewModel { Token = token, Email = email });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null) return RedirectToAction("ResetPasswordConfirmation");

                var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

                if (result.Succeeded) return RedirectToAction("ResetPasswordConfirmation");

                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error resetting password for {model.Email}");
                ModelState.AddModelError("", "An unexpected error occurred while resetting password.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation() => View();
    }
}
