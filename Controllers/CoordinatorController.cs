using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContractClaimSystem.Models;
using System.Linq;
using System.Threading.Tasks;

public class CoordinatorController : Controller
{
    private readonly ApplicationDbContext _context;

    public CoordinatorController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Coordinator Dashboard
    // Coordinator Dashboard
    public IActionResult Dashboard(string status)
    {
        ViewBag.StatusFilter = status ?? "";

        var claims = _context.Claims
            .Where(c => string.IsNullOrEmpty(status) || c.Status.ToString() == status)
            .ToList();

        return View(claims);
    }



    // Review Claims
    public async Task<IActionResult> ReviewClaims(string statusFilter = "Pending")
    {
        // Fetch claims based on filter
        var claimsQuery = _context.Claims
            .Include(c => c.Lecturer)
            .Include(c => c.SupportingDocuments)
            .AsQueryable();

        if (!string.IsNullOrEmpty(statusFilter))
        {
            if (Enum.TryParse(statusFilter, out ClaimStatus parsedStatus))
            {
                claimsQuery = claimsQuery.Where(c => c.Status == parsedStatus);
            }
        }

        var claims = await claimsQuery
            .OrderByDescending(c => c.SubmissionDate)
            .ToListAsync();

        ViewBag.StatusFilter = statusFilter;

        return View(claims);
    }

    // Track Submissions
    public IActionResult TrackSubmissions()
    {
        return View();
    }
}
