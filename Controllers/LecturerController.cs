using Microsoft.AspNetCore.Mvc;

public class LecturerController : Controller
{
    // Lecturer Dashboard
    public IActionResult Dashboard()
    {
        return View();
    }

    // Claim History
    public IActionResult ClaimHistory()
    {
        return View();
    }

    // Submit Claim
    public IActionResult SubmitClaim()
    {
        return View();
    }

    // Upload Document
    public IActionResult UploadDocument()
    {
        return View();
    }

    // All Claims
    // Submitted Claims (only claims submitted by this lecturer)
public IActionResult SubmittedClaims()
{
    return View();
}

}
