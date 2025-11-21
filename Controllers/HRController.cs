using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContractClaimSystem.Models;
using ContractClaimSystem.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContractClaimSystem.Controllers
{
    [Authorize(Roles = "HR")]
    public class HRController : Controller
    {
        private readonly ClaimService _claimService;
        private readonly LecturerService _lecturerService;

        public HRController(ClaimService claimService, LecturerService lecturerService)
        {
            _claimService = claimService;
            _lecturerService = lecturerService;
        }

        // Landing page for HR - redirect to dashboard
        public IActionResult Index()
        {
            return RedirectToAction("HRDashboard");
        }

        // Main HR dashboard
        public async Task<IActionResult> HRDashboard(string? status)
        {
            ClaimStatus? parsedStatus = null;

            if (!string.IsNullOrEmpty(status) &&
                Enum.TryParse<ClaimStatus>(status, true, out var result))
            {
                parsedStatus = result;
            }

            var claims = await _claimService.GetClaimsAsync(parsedStatus);
            var lecturers = await _lecturerService.GetLecturersAsync();

            var model = new HRDashboardViewModel
            {
                Claims = claims,
                Lecturers = lecturers,
                TotalApprovedClaims = claims.Count(c => c.Status == ClaimStatus.Approved),
                TotalPayment = claims.Where(c => c.Status == ClaimStatus.Approved)
                                     .Sum(c => c.HoursWorked * c.HourlyRate)
            };

            return View(model); // Points to HRDashboard.cshtml
        }

        // Approve selected claims
        [HttpPost]
        public async Task<IActionResult> ApproveClaims(List<int> claimIds)
        {
            if (claimIds != null && claimIds.Count > 0)
            {
                await _claimService.ApproveClaimsAsync(claimIds);
                TempData["SuccessMessage"] = $"{claimIds.Count} claim(s) approved successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "No claims selected to approve.";
            }

            return RedirectToAction("HRDashboard");
        }

        // Summary report of approved claims
        public async Task<IActionResult> SummaryReport()
        {
            var approvedClaims = await _claimService.GetClaimsAsync(ClaimStatus.Approved);
            var lecturers = await _lecturerService.GetLecturersAsync();

            // Map lecturer names safely
            var summary = approvedClaims
                .GroupBy(c => c.LecturerId)
                .Select(g =>
                {
                    var lecturer = lecturers.FirstOrDefault(l => l.Id == g.Key);
                    return new HRClaimSummaryViewModel
                    {
                        LecturerName = lecturer?.FullName ?? "Unknown",
                        TotalHours = g.Sum(c => c.HoursWorked),
                        TotalPayment = g.Sum(c => c.HoursWorked * c.HourlyRate)
                    };
                })
                .ToList();

            var grandTotalHours = summary.Sum(s => s.TotalHours);
            var grandTotalPayment = summary.Sum(s => s.TotalPayment);

            var model = new HRSummaryViewModel
            {
                LecturerSummaries = summary,
                GrandTotalHours = grandTotalHours,
                GrandTotalPayment = grandTotalPayment
            };

            return View(model); // Points to SummaryReport.cshtml
        }

        // Update lecturer info
        [HttpPost]
        public async Task<IActionResult> UpdateLecturer(string userId, string email, string phone)
        {
            if (!string.IsNullOrEmpty(userId))
            {
                await _lecturerService.UpdateLecturerInfoAsync(userId, email, phone);
                TempData["SuccessMessage"] = $"Lecturer {userId} updated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Invalid lecturer ID.";
            }

            return RedirectToAction("HRDashboard");
        }
    }
}
