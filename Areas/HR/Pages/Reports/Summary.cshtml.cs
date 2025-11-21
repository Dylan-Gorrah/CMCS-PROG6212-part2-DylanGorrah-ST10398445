using CMS_ASSIGNMENT.Interfaces;
using CMS_ASSIGNMENT.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CMS_ASSIGNMENT.Areas.HR.Pages.Reports
{
    [Authorize(Roles = "HR")]
    public class SummaryModel : PageModel
    {
        private readonly IReportService _reportService;

        public SummaryModel(IReportService reportService)
        {
            _reportService = reportService;
        }

        [BindProperty(SupportsGet = true)]
        public int? Year { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? Month { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        public ReportViewModel? Report { get; private set; }

        public async Task OnGetAsync()
        {
            if (Month.HasValue && Year.HasValue)
            {
                Report = await _reportService.GenerateMonthlyReportAsync(Year.Value, Month.Value);
                return;
            }

            if (StartDate.HasValue && EndDate.HasValue)
            {
                if (EndDate < StartDate)
                {
                    TempData["ErrorMessage"] = "End date cannot be earlier than start date.";
                    return;
                }

                Report = await _reportService.GenerateDateRangeReportAsync(StartDate.Value, EndDate.Value);
            }
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostDownloadAsync(int? year, int? month, DateTime? startDate, DateTime? endDate)
        {
            try
            {
                ReportViewModel report;

                if (month.HasValue && year.HasValue)
                {
                    report = await _reportService.GenerateMonthlyReportAsync(year.Value, month.Value);
                }
                else if (startDate.HasValue && endDate.HasValue)
                {
                    if (endDate < startDate)
                    {
                        throw new InvalidOperationException("End date cannot be earlier than start date.");
                    }

                    report = await _reportService.GenerateDateRangeReportAsync(startDate.Value, endDate.Value);
                }
                else
                {
                    throw new InvalidOperationException("Specify a month or a date range before downloading.");
                }

                var bytes = await _reportService.ExportReportToPdfAsync(report);
                var fileName = month.HasValue && year.HasValue
                    ? $"claims-report-{year}-{month:D2}.html"
                    : $"claims-report-{DateTime.Now:yyyyMMddHHmmss}.html";

                return File(bytes, "text/html", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToPage(new { Year = year, Month = month, StartDate = startDate?.ToString("yyyy-MM-dd"), EndDate = endDate?.ToString("yyyy-MM-dd") });
            }
        }
    }
}
