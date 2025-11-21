using CMS_ASSIGNMENT.Data;
using CMS_ASSIGNMENT.Interfaces;
using CMS_ASSIGNMENT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace CMS_ASSIGNMENT.Areas.HR.Pages.Reports
{
    [Authorize(Roles = "HR")]
    public class InvoicesModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IReportService _reportService;

        public InvoicesModel(ApplicationDbContext context, IReportService reportService)
        {
            _context = context;
            _reportService = reportService;
        }

        public IList<ApprovedClaimListItem> Claims { get; private set; } = new List<ApprovedClaimListItem>();

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } = string.Empty;

        public async Task OnGetAsync()
        {
            var claimsQuery = _context.Claims
                .Include(c => c.Lecturer)
                .Where(c => c.Status == ClaimStatus.ApprovedByManager);

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var term = SearchTerm.Trim().ToLowerInvariant();

                claimsQuery = claimsQuery.Where(c =>
                    c.LecturerName.ToLower().Contains(term) ||
                    (c.Lecturer != null && c.Lecturer.Email != null && c.Lecturer.Email.ToLower().Contains(term)) ||
                    c.Id.ToString().Contains(term));
            }

            Claims = await claimsQuery
                .OrderByDescending(c => c.ApprovedDate ?? c.SubmittedDate)
                .Select(c => new ApprovedClaimListItem
                {
                    Id = c.Id,
                    LecturerName = c.LecturerName,
                    LecturerEmail = c.Lecturer != null ? c.Lecturer.Email ?? string.Empty : string.Empty,
                    ApprovedDate = c.ApprovedDate,
                    TotalAmount = c.TotalAmount,
                    IsFlaggedForReview = c.IsFlaggedForReview,
                    FlaggedReasons = c.FlaggedReasons
                })
                .ToListAsync();
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostDownloadAsync(int claimId)
        {
            try
            {
                var invoice = await _reportService.GenerateInvoiceAsync(claimId);
                var bytes = await _reportService.ExportInvoiceToPdfAsync(invoice);
                var fileName = $"{invoice.InvoiceNumber}.html";
                return File(bytes, "text/html", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage(new { SearchTerm });
            }
        }

        public record ApprovedClaimListItem
        {
            public int Id { get; init; }
            public string LecturerName { get; init; } = string.Empty;
            public string LecturerEmail { get; init; } = string.Empty;
            public DateTime? ApprovedDate { get; init; }
            public decimal TotalAmount { get; init; }
            public bool IsFlaggedForReview { get; init; }
            public string? FlaggedReasons { get; init; }
        }
    }
}
