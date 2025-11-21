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
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public IndexModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IList<LecturerListItem> Lecturers { get; private set; } = new List<LecturerListItem>();

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string Filter { get; set; } = "all";

        public async Task OnGetAsync()
        {
            var lecturersQuery = _userManager.Users
                .Where(u => u.Role == UserRole.Lecturer);

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var term = SearchTerm.ToLowerInvariant();
                lecturersQuery = lecturersQuery.Where(u =>
                    u.FirstName.ToLower().Contains(term) ||
                    u.LastName.ToLower().Contains(term) ||
                    u.Email.ToLower().Contains(term));
            }

            var lecturers = await lecturersQuery
                .Select(u => new LecturerListItem
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email ?? string.Empty,
                    PhoneNumber = u.PhoneNumber,
                    TotalClaims = _context.Claims.Count(c => c.LecturerId == u.Id),
                    ApprovedClaims = _context.Claims.Count(c => c.LecturerId == u.Id && c.Status == ClaimStatus.ApprovedByManager),
                    TotalApprovedAmount = _context.Claims.Where(c => c.LecturerId == u.Id && c.Status == ClaimStatus.ApprovedByManager).Sum(c => (decimal?)c.TotalAmount) ?? 0,
                    FlaggedClaims = _context.Claims.Count(c => c.LecturerId == u.Id && c.IsFlaggedForReview),
                    LastSubmitted = _context.Claims.Where(c => c.LecturerId == u.Id).OrderByDescending(c => c.SubmittedDate).Select(c => (DateTime?)c.SubmittedDate).FirstOrDefault()
                })
                .ToListAsync();

            Lecturers = Filter switch
            {
                "withClaims" => lecturers.Where(l => l.TotalClaims > 0).ToList(),
                "withoutClaims" => lecturers.Where(l => l.TotalClaims == 0).ToList(),
                "flagged" => lecturers.Where(l => l.FlaggedClaims > 0).ToList(),
                _ => lecturers
            };
        }

        public async Task<IActionResult> OnPostDeleteAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["ErrorMessage"] = "Invalid lecturer identifier.";
                return RedirectToPage();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null || user.Role != UserRole.Lecturer)
            {
                TempData["ErrorMessage"] = "Unable to locate the lecturer.";
                return RedirectToPage(new { SearchTerm, Filter });
            }

            var linkedClaims = await _context.Claims.CountAsync(c => c.LecturerId == id);
            if (linkedClaims > 0)
            {
                TempData["ErrorMessage"] = "Cannot delete lecturer while claims exist. Reassign or remove related claims first.";
                return RedirectToPage(new { SearchTerm, Filter });
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Lecturer {user.FullName} deleted.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete lecturer.";
            }

            return RedirectToPage(new { SearchTerm, Filter });
        }

        public record LecturerListItem
        {
            public string Id { get; init; } = string.Empty;
            public string FullName { get; init; } = string.Empty;
            public string Email { get; init; } = string.Empty;
            public string? PhoneNumber { get; init; }
            public int TotalClaims { get; init; }
            public int ApprovedClaims { get; init; }
            public decimal TotalApprovedAmount { get; init; }
            public int FlaggedClaims { get; init; }
            public DateTime? LastSubmitted { get; init; }
        }
    }
}
