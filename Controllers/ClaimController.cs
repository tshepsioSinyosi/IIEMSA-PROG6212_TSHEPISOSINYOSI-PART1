using Microsoft.AspNetCore.Mvc;
using ContractClaimSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ContractClaimSystem.Controllers
{
    public class ClaimController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ClaimController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Claim/Submit
        [HttpGet]
        public IActionResult Submit()
        {
            return View();
        }

        // POST: Claim/Submit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(ClaimSubmissionViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Create Claim entity
            var claim = new ContractClaimSystem.Models.Claim
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

            // Handle supporting documents
            if (model.SupportingFiles != null && model.SupportingFiles.Count > 0)
            {
                foreach (var file in model.SupportingFiles)
                {
                    if (file.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        var filePath = Path.Combine(uploadsFolder, file.FileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        var doc = new SupportingDocument
                        {
                            ClaimId = claim.ClaimId,
                            FileName = file.FileName,
                            FilePath = "/uploads/" + file.FileName
                        };

                        _context.SupportingDocuments.Add(doc);
                    }
                }
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Your claim was successfully submitted!";
            return RedirectToAction("MyClaims");
        }

        // GET: Claim/MyClaims
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

        // GET: Claim/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var claim = await _context.Claims
                .Include(c => c.SupportingDocuments)
                .Include(c => c.Lecturer)
                .FirstOrDefaultAsync(c => c.ClaimId == id);

            if (claim == null) return NotFound();

            return View(claim);
        }
    }
}
