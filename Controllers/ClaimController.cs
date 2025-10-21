// Controllers/ClaimController.cs
using System.Security.Claims;
using ContractMonthlyClaimsSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


[Authorize(Roles = "Lecturer")]
public class ClaimController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<User> _userManager;
    private readonly IFileStorageService _fileStorage;

    public ClaimController(ApplicationDbContext db, UserManager<User> userManager, IFileStorageService fileStorage)
    {
        _db = db;
        _userManager = userManager;
        _fileStorage = fileStorage;
    }

    [HttpGet]
    public IActionResult Submit()
    {
        return View(new ClaimSubmissionViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(ClaimSubmissionViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var userId = _userManager.GetUserId(User); // works with Identity

        var claim = new Claim
        {
            LecturerId = userId,
            HoursWorked = vm.HoursWorked,
            HourlyRate = vm.HourlyRate,
            Notes = vm.Notes,
            SubmissionDate = DateTime.UtcNow,
            Status = ClaimStatus.Pending
        };

        try
        {
            _db.Claims.Add(claim);
            await _db.SaveChangesAsync(); // so claim.Id is generated

            if (vm.SupportingFiles != null && vm.SupportingFiles.Count > 0)
            {
                foreach (var file in vm.SupportingFiles)
                {
                    // Basic checks: size and extension
                    if (file.Length > 5 * 1024 * 1024) // 5 MB limit
                    {
                        // optionally delete claim and return error
                        ModelState.AddModelError("", "One of the files exceeds 5 MB.");
                        return View(vm);
                    }

                    // allowed types simple check
                    var allowed = new[] { ".pdf", ".png", ".jpg", ".jpeg", ".docx" };
                    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!allowed.Contains(ext))
                    {
                        ModelState.AddModelError("", $"File type {ext} is not allowed.");
                        return View(vm);
                    }

                    var (storedFileName, savedPath) = await _fileStorage.SaveFileAsync(file, "claims");

                    var doc = new SupportingDocument
                    {
                        ClaimId = claim.Id,
                        FileName = file.FileName,
                        StoredFileName = storedFileName,
                        FilePath = savedPath,
                        UploadDate = DateTime.UtcNow
                    };

                    _db.SupportingDocuments.Add(doc);
                }

                await _db.SaveChangesAsync();
            }

            TempData["Success"] = "Claim submitted successfully.";
            return RedirectToAction(nameof(MyClaims));
        }
        catch (Exception ex)
        {
            // log ex
            ModelState.AddModelError("", "An unexpected error occurred while saving. Please try again.");
            return View(vm);
        }
    }

    // Lecturer can view their claims
    [HttpGet]
    public async Task<IActionResult> MyClaims()
    {
        var userId = _userManager.GetUserId(User);
        var claims = await _db.Claims
            .Where(c => c.LecturerId == userId)
            .OrderByDescending(c => c.SubmissionDate)
            .ToListAsync();

        return View(claims);
    }

    // download supporting doc (authorize to own claim OR roles)
    [HttpGet]
    public async Task<IActionResult> DownloadDoc(int id)
    {
        var doc = await _db.SupportingDocuments.Include(d => d.Claim).FirstOrDefaultAsync(d => d.Id == id);
        if (doc == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        var isOwner = doc.Claim.LecturerId == userId;
        var isCoordinatorOrManager = User.IsInRole("Coordinator") || User.IsInRole("Manager");

        if (!isOwner && !isCoordinatorOrManager) return Forbid();

        var bytes = await System.IO.File.ReadAllBytesAsync(doc.FilePath);
        return File(bytes, "application/octet-stream", doc.FileName);
    }
}
