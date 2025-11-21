using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

using ClaimEntity = ContractClaimSystem.Models.Claim;
using ContractClaimSystem.Models;

namespace ContractClaimSystem.Controllers
{
    [Authorize]
    public class ClaimController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<ClaimController> _logger;

        public ClaimController(
            ApplicationDbContext context,
            IFileStorageService fileStorageService,
            ILogger<ClaimController> logger)
        {
            _context = context;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        // GET: Show submission form (uses ClaimSubmissionViewModel)
        [HttpGet]
        [Authorize(Roles = "Lecturer")]
        public IActionResult SubmitClaim()
        {
            var vm = new ClaimSubmissionViewModel();
            return View(vm);
        }

        // POST: Submit claim (accepts ClaimSubmissionViewModel)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Lecturer")]
        public async Task<IActionResult> SubmitClaim(ClaimSubmissionViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Some fields are missing or invalid.";
                    return View("SubmitClaim", vm);
                }

                // Get logged-in lecturer id
                var lecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(lecturerId))
                {
                    TempData["Error"] = "User is not logged in.";
                    _logger.LogWarning("Submit: no logged-in user found.");
                    return View("SubmitClaim", vm);
                }

                // Ensure the user exists in DB
                var lecturerExists = await _context.Users.AnyAsync(u => u.Id == lecturerId);
                if (!lecturerExists)
                {
                    TempData["Error"] = "Lecturer not found in system.";
                    _logger.LogWarning("Submit: lecturer {LecturerId} not present in Users table.", lecturerId);
                    return View("SubmitClaim", vm);
                }

                // Map ViewModel -> Entity
                var claim = new ClaimEntity
                {
                    LecturerId = lecturerId,
                    HoursWorked = vm.HoursWorked,
                    HourlyRate = vm.HourlyRate,
                    AdditionalNotes = vm.Notes ?? string.Empty,
                    SubmissionDate = DateTime.UtcNow,
                    Status = ClaimStatus.Pending
                };
                claim.RequiresReview = false;

                if (claim.HourlyRate < 100 || claim.HourlyRate > 500 || claim.HoursWorked < 5 || claim.HoursWorked > 160)
                {
                    claim.RequiresReview = true;
                }
                if (vm.SupportingFiles == null || !vm.SupportingFiles.Any())
                {
                    claim.RequiresReview = true;
                }

                if (!claim.RequiresReview)
                {
                    claim.Status = ClaimStatus.Approved; // Auto-approve
                }
                else
                {
                    claim.Status = ClaimStatus.Pending; // Needs coordinator review
                }


                // Save claim first to get ClaimId for supporting documents
                _context.Claims.Add(claim);
                await _context.SaveChangesAsync();

                // Handle file uploads (if any)
                if (vm.SupportingFiles != null && vm.SupportingFiles.Any())
                {
                    foreach (var file in vm.SupportingFiles)
                    {
                        if (file == null || file.Length == 0) continue;

                        // NOTE: IFileStorageService.SaveFileAsync expected to return a tuple:
                        //    Task<(string storedFileName, string filePath)>
                        // If your implementation returns just string filePath, change the line below accordingly.
                        var (storedFileName, filePath) = await _fileStorageService.SaveFileAsync(file);

                        var doc = new SupportingDocument
                        {
                            ClaimId = claim.ClaimId,
                            FileName = storedFileName,
                            FilePath = filePath
                        };

                        _context.SupportingDocuments.Add(doc);
                    }

                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Claim submitted successfully!";
                _logger.LogInformation("Claim {ClaimId} submitted by {LecturerId}.", claim.ClaimId, lecturerId);
                return RedirectToAction("MyClaims");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERROR submitting claim for user {User}", User?.Identity?.Name ?? "unknown");
                TempData["Error"] = "Unexpected error while submitting claim: " + ex.Message;
                return View("SubmitClaim", vm);
            }
        }

        // Lecturer's claims
        [Authorize(Roles = "Lecturer")]
        public async Task<IActionResult> MyClaims()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var claims = await _context.Claims
                .Include(c => c.SupportingDocuments)
                .Where(c => c.LecturerId == userId)
                .OrderByDescending(c => c.SubmissionDate)
                .ToListAsync();

            return View(claims);
        }

        // Other actions unchanged...
        [Authorize(Roles = "Coordinator,Manager")]
        public async Task<IActionResult> PendingClaims()
        {
            var claims = await _context.Claims
                .Include(c => c.Lecturer)
                .Include(c => c.SupportingDocuments)
                .Where(c => c.Status == ClaimStatus.Pending)
                .OrderBy(c => c.SubmissionDate)
                .ToListAsync();

            return View(claims);
        }

        [Authorize(Roles = "Coordinator,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null) return NotFound();

            claim.Status = ClaimStatus.Approved;
            claim.ReviewDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "✅ Claim approved!";
            return RedirectToAction(nameof(PendingClaims));
        }

        [Authorize(Roles = "Coordinator,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null) return NotFound();

            claim.Status = ClaimStatus.Rejected;
            claim.ReviewDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["ErrorMessage"] = "❌ Claim rejected!";
            return RedirectToAction(nameof(PendingClaims));
        }

        public async Task<IActionResult> Details(int id)
        {
            var claim = await _context.Claims
                .Include(c => c.Lecturer)
                .Include(c => c.SupportingDocuments)
                .FirstOrDefaultAsync(c => c.ClaimId == id);

            if (claim == null) return NotFound();
            return View(claim);
        }

        [HttpGet]
        [Authorize(Roles = "Lecturer")]
        public async Task<IActionResult> Edit(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null || claim.Status != ClaimStatus.Pending) return NotFound();

            return View(claim);
        }

        [HttpPost]
        [Authorize(Roles = "Lecturer")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClaimEntity updated)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null) return NotFound();

            claim.HoursWorked = updated.HoursWorked;
            claim.HourlyRate = updated.HourlyRate;
            claim.AdditionalNotes = updated.AdditionalNotes;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "✏️ Claim updated successfully!";
            return RedirectToAction(nameof(MyClaims)); 
        }

        [Authorize(Roles = "Lecturer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null) return NotFound();

            var docs = _context.SupportingDocuments.Where(d => d.ClaimId == id).ToList();
            foreach (var doc in docs)
            {
                try
                {
                    await _fileStorageService.DeleteFileAsync(doc.FilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed deleting file {FilePath}", doc.FilePath);
                }
            }
            _context.SupportingDocuments.RemoveRange(docs); 

            _context.Claims.Remove(claim);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "🗑️ Claim deleted.";
            return RedirectToAction(nameof(MyClaims));
        }
    }
}
