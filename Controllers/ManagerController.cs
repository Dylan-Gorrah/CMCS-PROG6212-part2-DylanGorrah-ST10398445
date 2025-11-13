using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CMS_ASSIGNMENT.Interfaces;
using CMS_ASSIGNMENT.ViewModels;
using CMS_ASSIGNMENT.Models;
using Microsoft.AspNetCore.Identity;

namespace CMS_ASSIGNMENT.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ManagerController : Controller
    {
        private readonly IClaimService _claimService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ManagerController(IClaimService claimService, UserManager<ApplicationUser> userManager)
        {
            _claimService = claimService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var pendingClaims = await _claimService.GetPendingClaimsForManagerAsync();
            var viewModel = pendingClaims.Select(c => new ClaimListViewModel
            {
                Id = c.Id,
                LecturerName = c.LecturerName,
                CoordinatorName = c.CoordinatorName,
                SubmittedDate = c.SubmittedDate,
                HoursWorked = c.HoursWorked,
                HourlyRate = c.HourlyRate,
                TotalAmount = c.TotalAmount,
                Status = c.Status.ToString(),
                DocumentFileName = c.DocumentFileName
            }).ToList();

            ViewBag.User = user;
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveClaim(int id)
        {
            var result = await _claimService.ApproveClaimByManagerAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Claim approved successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to approve claim. It may have been already processed.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectClaim(int id)
        {
            var result = await _claimService.RejectClaimByManagerAsync(id);
            if (result)
            {
                TempData["SuccessMessage"] = "Claim rejected successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to reject claim. It may have been already processed.";
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DownloadDocument(int id)
        {
            var claim = await _claimService.GetClaimByIdAsync(id);

            if (claim == null)
            {
                return NotFound();
            }

            if (string.IsNullOrEmpty(claim.DocumentFilePath) || !System.IO.File.Exists(claim.DocumentFilePath))
            {
                TempData["ErrorMessage"] = "Document not found.";
                return RedirectToAction("Index");
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(claim.DocumentFilePath);
            return File(fileBytes, "application/octet-stream", claim.DocumentFileName ?? "document");
        }

        public async Task<IActionResult> Reports()
        {
            var user = await _userManager.GetUserAsync(User);
            ViewBag.User = user;

            return View();
        }
    }
}