using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContractClaimSystem.Models;
using System.Security.Claims;
using AppClaim = ContractClaimSystem.Models.Claim;

namespace ContractClaimSystem.Controllers
{
    [Authorize]
    public class ClaimController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileStorageService _fileStorageService;

        public ClaimController(ApplicationDbContext context, IFileStorageService fileStorageService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
        }

        // ===========================
        // 1️⃣ SUBMIT CLAIM (Lecturer)
        // ===========================
        [HttpGet]
        [Authorize(Roles = "Lecturer")]
        public IActionResult Submit()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Lecturer")]
        public async Task<IActionResult> Submit(ClaimSubmissionViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                // 1️⃣ Validate hours
                if (model.HoursWorked <= 0)
                {
                    ModelState.AddModelError("HoursWorked", "Hours worked must be greater than 0.");
                    return View(model);
                }

                // 2️⃣ Validate rate
                if (model.HourlyRate <= 0)
                {
                    ModelState.AddModelError("HourlyRate", "Hourly rate must be greater than 0.");
                    return View(model);
                }

                // 3️⃣ Auto-calc (server enforced)
                var calculatedTotal = model.HoursWorked * model.HourlyRate;

                if (calculatedTotal <= 0)
                {
                    ModelState.AddModelError("", "Total amount cannot be zero or negative.");
                    return View(model);
                }

                // 4️⃣ Ensure ONLY the calculated value is used
                // (Prevents users from editing the value using browser tools)
                decimal finalTotalAmount = calculatedTotal;

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // 5️⃣ Ensure a claim can only be submitted once per day
                bool alreadySubmitted = await _context.Claims
                    .AnyAsync(c => c.LecturerId == userId &&
                                   c.SubmissionDate.Date == DateTime.Now.Date);

                if (alreadySubmitted)
                {
                    ModelState.AddModelError("", "❌ You already submitted a claim today.");
                    return View(model);
                }

                // 6️⃣ Create Claim entity
                var claim = new AppClaim
                {
                    LecturerId = userId,
                    HoursWorked = model.HoursWorked,
                    HourlyRate = model.HourlyRate,
                    TotalAmount = finalTotalAmount,   // Automation enforced here
                    AdditionalNotes = model.Notes,
                    SubmissionDate = DateTime.Now,
                    Status = ClaimStatus.Pending
                };

                _context.Claims.Add(claim);
                await _context.SaveChangesAsync();

                // 7️⃣ Handle file uploads
                if (model.SupportingFiles != null && model.SupportingFiles.Count > 0)
                {
                    foreach (var file in model.SupportingFiles)
                    {
                        if (file.Length > 0)
                        {
                            var (storedFile, filePath) = await _fileStorageService.SaveFileAsync(file);

                            var doc = new SupportingDocument
                            {
                                ClaimId = claim.ClaimId,
                                FileName = file.FileName,
                                FilePath = filePath,
                                UploadDate = DateTime.UtcNow
                            };

                            _context.SupportingDocuments.Add(doc);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "✅ Claim submitted successfully!";
                return RedirectToAction(nameof(MyClaims));
            }
            catch (Exception ex)
            {
                // ⭐ CLEAR AUTOMATED ERROR FEEDBACK
                TempData["ErrorMessage"] = $"❌ An error occurred: {ex.Message}";
                return View(model);
            }
        }

        // ===========================
        // 2️⃣ VIEW CLAIMS (Lecturer)
        // ===========================
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

        // ===========================
        // 3️⃣ VIEW PENDING (Coordinator & Manager)
        // ===========================
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

        // ===========================
        // 4️⃣ APPROVE / REJECT ACTIONS
        // ===========================
        [Authorize(Roles = "Coordinator,Manager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null) return NotFound();

            claim.Status = ClaimStatus.Approved;
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
            await _context.SaveChangesAsync();

            TempData["ErrorMessage"] = "❌ Claim rejected!";
            return RedirectToAction(nameof(PendingClaims));
        }

        // ===========================
        // 5️⃣ CLAIM DETAILS
        // ===========================
        public async Task<IActionResult> Details(int id)
        {
            var claim = await _context.Claims
                .Include(c => c.Lecturer)
                .Include(c => c.SupportingDocuments)
                .FirstOrDefaultAsync(c => c.ClaimId == id);

            if (claim == null) return NotFound();
            return View(claim);
        }

        // ===========================
        // 6️⃣ EDIT (Lecturer only)
        // ===========================
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
        public async Task<IActionResult> Edit(int id, AppClaim updated)
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

        // ===========================
        // 7️⃣ DELETE (Lecturer only)
        // ===========================
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
                await _fileStorageService.DeleteFileAsync(doc.FilePath);
            }
            _context.SupportingDocuments.RemoveRange(docs);

            _context.Claims.Remove(claim);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "🗑️ Claim deleted.";
            return RedirectToAction(nameof(MyClaims));
        }
    }
}
