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

        // Main HR dashboard
        public async Task<IActionResult> Index(string? status)
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

            return View(model);
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

            return RedirectToAction("Index");
        }

        // Generate PDF/Excel report
        public async Task<IActionResult> GenerateReport()
        {
            try
            {
                var reportBytes = await _claimService.GenerateInvoiceReportAsync();
                return File(reportBytes, "application/pdf", "ApprovedClaims.pdf");
            }
            catch
            {
                TempData["ErrorMessage"] = "Failed to generate report. Please try again.";
                return RedirectToAction("Index");
            }
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

            return RedirectToAction("Index");
        }
    }
}
