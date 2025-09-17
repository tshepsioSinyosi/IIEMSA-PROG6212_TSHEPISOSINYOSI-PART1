using Microsoft.AspNetCore.Mvc;

public class ManagerController : Controller
{
    // Manager Dashboard
    public IActionResult Dashboard()
    {
        return View();
    }

    // Claim Approval
    public IActionResult ApproveClaims()
    {
        return View();
    }

    // Track Claims
    public IActionResult TrackClaims()
    {
        return View();
    }
}
