﻿using Microsoft.AspNetCore.Mvc;

public class CoordinatorController : Controller
{
    // Coordinator Dashboard
    public IActionResult Dashboard()
    {
        return View();
    }

    // Review Claims
    public IActionResult ReviewClaims()
    {
        return View();
    }

    // Track Submissions
    public IActionResult TrackSubmissions()
    {
        return View();
    }
}
