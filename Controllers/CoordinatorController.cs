using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CMS_ASSIGNMENT.Interfaces;
using CMS_ASSIGNMENT.ViewModels;
using CMS_ASSIGNMENT.Models;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace CMS_ASSIGNMENT.Controllers
{
    [Authorize(Roles = "Coordinator")]
    public class CoordinatorController : Controller
    {
        private readonly IClaimService _claimService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CoordinatorController(IClaimService claimService, UserManager<ApplicationUser> userManager)
        {
            _claimService = claimService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var pendingClaims = await _claimService.GetPendingClaimsForCoordinatorAsync();
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
                DocumentFileName = c.DocumentFileName,
                IsFlaggedForReview = c.IsFlaggedForReview,
                HasBlockingViolations = c.HasBlockingViolations,
                FlaggedReasons = c.FlaggedReasons
            }).ToList();

            ViewBag.PendingNotification = new
            {
                Total = viewModel.Count,
                Flagged = viewModel.Count(c => c.IsFlaggedForReview),
                Blocking = viewModel.Count(c => c.HasBlockingViolations)
            };

            ViewBag.User = user;
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveClaim(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var claim = await _claimService.GetClaimByIdAsync(id);
            if (claim == null || claim.CoordinatorId != user.Id)
            {
                TempData["ErrorMessage"] = "Unable to locate the requested claim.";
                return RedirectToAction("Index");
            }

            if (claim.HasBlockingViolations)
            {
                TempData["ErrorMessage"] = string.IsNullOrWhiteSpace(claim.FlaggedReasons)
                    ? "Claim contains blocking policy violations and cannot be approved."
                    : $"Claim cannot be approved: {claim.FlaggedReasons}";
                return RedirectToAction("Index");
            }

            var result = await _claimService.ApproveClaimByCoordinatorAsync(id, user.Id);
            if (result)
            {
                TempData["SuccessMessage"] = claim.IsFlaggedForReview
                    ? "Claim approved with caution – review alerts noted."
                    : "Claim approved successfully!";
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
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var result = await _claimService.RejectClaimByCoordinatorAsync(id, user.Id);
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
            var user = await _userManager.GetUserAsync(User);

            if (claim == null || claim.CoordinatorId != user?.Id)
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
    }
}