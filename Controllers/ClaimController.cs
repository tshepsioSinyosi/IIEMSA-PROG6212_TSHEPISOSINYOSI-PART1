using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContractClaimSystem.Models;
using System.Security.Claims;

namespace ContractClaimSystem.Controllers
{
    [Authorize]
    public class ClaimController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ClaimController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
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
            if (!ModelState.IsValid)
                return View(model);

            // ✅ Create Claim
            var claim = new Models.Claim
            {
                LecturerId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                HoursWorked = model.HoursWorked,
                HourlyRate = model.HourlyRate,
                AdditionalNotes = model.Notes,
                SubmissionDate = DateTime.Now,
                Status = ClaimStatus.Pending
            };

            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();

            // ================================
            // ✅ Handle File Upload (Validated)
            // ================================
            if (model.SupportingFiles != null && model.SupportingFiles.Count > 0)
            {
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx" };
                var maxSize = 5 * 1024 * 1024; // 5 MB

                foreach (var file in model.SupportingFiles)
                {
                    if (file.Length <= 0) continue;

                    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

                    // ❌ Reject invalid extension
                    if (!allowedExtensions.Contains(ext))
                    {
                        TempData["ErrorMessage"] = $"File '{file.FileName}' is not allowed. Only PDF, DOCX, and XLSX are accepted.";
                        return View(model);
                    }

                    // ❌ Reject large files
                    if (file.Length > maxSize)
                    {
                        TempData["ErrorMessage"] = $"File '{file.FileName}' exceeds 5 MB limit.";
                        return View(model);
                    }

                    // ✅ Save file
                    var uniqueFileName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // ✅ Save document record in DB
                    var doc = new SupportingDocument
                    {
                        ClaimId = claim.ClaimId,
                        FileName = file.FileName, // original name
                        FilePath = "/uploads/" + uniqueFileName // unique name on disk
                    };

                    _context.SupportingDocuments.Add(doc);
                }
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "✅ Claim submitted successfully!";
            return RedirectToAction(nameof(MyClaims));
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
        // 5️⃣ DETAILS
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
        public async Task<IActionResult> Edit(int id, Models.Claim updated)
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
        public async Task<IActionResult> Delete(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null) return NotFound();

            // Delete supporting files from disk too
            var docs = _context.SupportingDocuments.Where(d => d.ClaimId == id).ToList();
            foreach (var doc in docs)
            {
                var path = Path.Combine(_environment.WebRootPath, doc.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
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
