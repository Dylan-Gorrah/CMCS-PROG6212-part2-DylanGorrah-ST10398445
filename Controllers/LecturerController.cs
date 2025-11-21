using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CMS_ASSIGNMENT.Interfaces;
using CMS_ASSIGNMENT.ViewModels;
using CMS_ASSIGNMENT.Models;
using Microsoft.AspNetCore.Identity;

namespace CMS_ASSIGNMENT.Controllers
{
    [Authorize(Roles = "Lecturer")]
    public class LecturerController : Controller
    {
        private readonly IClaimService _claimService;
        private readonly UserManager<ApplicationUser> _userManager;

        public LecturerController(IClaimService claimService, UserManager<ApplicationUser> userManager)
        {
            _claimService = claimService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var claims = await _claimService.GetClaimsByLecturerAsync(user.Id);
            var viewModel = claims.Select(c => new ClaimListViewModel
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

            ViewBag.User = user;
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitClaim(ClaimViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return RedirectToAction("Login", "Account");

                // Get the coordinator for this lecturer
                var coordinators = await _userManager.GetUsersInRoleAsync("Coordinator");
                var assignedCoordinator = coordinators.FirstOrDefault();

                if (assignedCoordinator == null)
                {
                    ModelState.AddModelError("", "No coordinator available to assign this claim.");
                    return View("Index");
                }

                var claim = new Claim
                {
                    LecturerId = user.Id,
                    LecturerName = user.FullName ?? "Unknown",
                    CoordinatorId = assignedCoordinator.Id,
                    CoordinatorName = assignedCoordinator.FullName ?? "Unknown",
                    HoursWorked = model.HoursWorked,
                    HourlyRate = model.HourlyRate,
                    Status = ClaimStatus.Pending,
                    SubmittedDate = DateTime.UtcNow
                };

                try
                {
                    await _claimService.SubmitClaimAsync(claim, model.Document);
                    TempData["SuccessMessage"] = "Claim submitted successfully!";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error submitting claim: {ex.Message}");
                }
            }

            // If we got this far, something failed; redisplay form
            var userForView = await _userManager.GetUserAsync(User);
            if (userForView == null) return RedirectToAction("Login", "Account");

            ViewBag.User = userForView;

            // Get claims again for the view
            var claims = await _claimService.GetClaimsByLecturerAsync(userForView.Id);
            var viewModel = claims.Select(c => new ClaimListViewModel
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

            return View("Index", viewModel);
        }

        public async Task<IActionResult> DownloadDocument(int id)
        {
            var claim = await _claimService.GetClaimByIdAsync(id);
            var user = await _userManager.GetUserAsync(User);

            if (claim == null || claim.LecturerId != user?.Id)
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