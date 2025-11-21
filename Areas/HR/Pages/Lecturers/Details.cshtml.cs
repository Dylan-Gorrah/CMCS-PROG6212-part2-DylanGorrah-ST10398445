using CMS_ASSIGNMENT.Data;
using CMS_ASSIGNMENT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CMS_ASSIGNMENT.Areas.HR.Pages.Lecturers
{
    [Authorize(Roles = "HR")]
    public class DetailsModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public DetailsModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public LecturerSummary Lecturer { get; private set; } = default!;
        public IList<ClaimSummary> Claims { get; private set; } = new List<ClaimSummary>();

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["ErrorMessage"] = "Invalid lecturer identifier.";
                return RedirectToPage("./Index");
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == UserRole.Lecturer);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Unable to locate the lecturer.";
                return RedirectToPage("./Index");
            }

            Lecturer = new LecturerSummary
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                TotalClaims = await _context.Claims.CountAsync(c => c.LecturerId == user.Id),
                ApprovedClaims = await _context.Claims.CountAsync(c => c.LecturerId == user.Id && c.Status == ClaimStatus.ApprovedByManager),
                PendingClaims = await _context.Claims.CountAsync(c => c.LecturerId == user.Id && (c.Status == ClaimStatus.Pending || c.Status == ClaimStatus.ApprovedByCoordinator)),
                FlaggedClaims = await _context.Claims.CountAsync(c => c.LecturerId == user.Id && c.IsFlaggedForReview),
                TotalApprovedAmount = await _context.Claims.Where(c => c.LecturerId == user.Id && c.Status == ClaimStatus.ApprovedByManager).SumAsync(c => (decimal?)c.TotalAmount) ?? 0,
                LastSubmitted = await _context.Claims.Where(c => c.LecturerId == user.Id).OrderByDescending(c => c.SubmittedDate).Select(c => (DateTime?)c.SubmittedDate).FirstOrDefaultAsync()
            };

            Claims = await _context.Claims
                .Where(c => c.LecturerId == user.Id)
                .OrderByDescending(c => c.SubmittedDate)
                .Select(c => new ClaimSummary
                {
                    Id = c.Id,
                    SubmittedDate = c.SubmittedDate,
                    HoursWorked = c.HoursWorked,
                    HourlyRate = c.HourlyRate,
                    TotalAmount = c.TotalAmount,
                    Status = c.Status.ToString(),
                    IsFlaggedForReview = c.IsFlaggedForReview,
                    HasBlockingViolations = c.HasBlockingViolations,
                    FlaggedReasons = c.FlaggedReasons,
                    AdditionalNotes = c.AdditionalNotes
                })
                .ToListAsync();

            return Page();
        }

        public record LecturerSummary
        {
            public string Id { get; init; } = string.Empty;
            public string FullName { get; init; } = string.Empty;
            public string Email { get; init; } = string.Empty;
            public string? PhoneNumber { get; init; }
            public int TotalClaims { get; init; }
            public int ApprovedClaims { get; init; }
            public int PendingClaims { get; init; }
            public int FlaggedClaims { get; init; }
            public decimal TotalApprovedAmount { get; init; }
            public DateTime? LastSubmitted { get; init; }
        }

        public record ClaimSummary
        {
            public int Id { get; init; }
            public DateTime SubmittedDate { get; init; }
            public decimal HoursWorked { get; init; }
            public decimal HourlyRate { get; init; }
            public decimal TotalAmount { get; init; }
            public string Status { get; init; } = string.Empty;
            public bool IsFlaggedForReview { get; init; }
            public bool HasBlockingViolations { get; init; }
            public string? FlaggedReasons { get; init; }
            public string? AdditionalNotes { get; init; }
        }
    }
}
