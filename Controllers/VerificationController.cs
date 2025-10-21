// Controllers/VerificationController.cs
using ContractMonthlyClaimsSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


[Authorize(Roles = "Coordinator,Manager")]
public class VerificationController : Controller
{
    private readonly ApplicationDbContext _db;

    public VerificationController(ApplicationDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> PendingClaims()
    {
        var pending = await _db.Claims
            .Where(c => c.Status == ClaimStatus.Pending)
            .Include(c => c.SupportingDocuments)
            .OrderBy(c => c.SubmissionDate)
            .ToListAsync();

        return View(pending);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveClaim(int id)
    {
        var claim = await _db.Claims.FindAsync(id);
        if (claim == null) return NotFound();

        claim.Status = ClaimStatus.Approved;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Claim approved";
        return RedirectToAction(nameof(PendingClaims));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectClaim(int id, string reason = null)
    {
        var claim = await _db.Claims.FindAsync(id);
        if (claim == null) return NotFound();

        claim.Status = ClaimStatus.Rejected;
        // optionally store rejection reason somewhere (add column if required)
        await _db.SaveChangesAsync();
        TempData["Success"] = "Claim rejected";
        return RedirectToAction(nameof(PendingClaims));
    }

    // optionally view claim details
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var claim = await _db.Claims
            .Include(c => c.SupportingDocuments)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (claim == null) return NotFound();
        return View(claim);
    }
}
