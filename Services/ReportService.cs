using CMS_ASSIGNMENT.Interfaces;
using CMS_ASSIGNMENT.ViewModels;
using CMS_ASSIGNMENT.Models;
using Microsoft.AspNetCore.Identity;
using System.Text;

namespace CMS_ASSIGNMENT.Services
{
    public class ReportService : IReportService
    {
        private readonly IClaimRepository _claimRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportService(IClaimRepository claimRepository, UserManager<ApplicationUser> userManager)
        {
            _claimRepository = claimRepository;
            _userManager = userManager;
        }

        public async Task<ReportViewModel> GenerateMonthlyReportAsync(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            return await GenerateDateRangeReportAsync(startDate, endDate);
        }

        public async Task<ReportViewModel> GenerateDateRangeReportAsync(DateTime startDate, DateTime endDate)
        {
            var allClaims = await _claimRepository.GetAllAsync();
            var filteredClaims = allClaims.Where(c =>
                c.SubmittedDate >= startDate &&
                c.SubmittedDate <= endDate.AddDays(1)
            ).ToList();

            var report = new ReportViewModel
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalClaims = filteredClaims.Count,
                ApprovedClaims = filteredClaims.Count(c => c.Status == ClaimStatus.ApprovedByManager),
                PendingClaims = filteredClaims.Count(c =>
                    c.Status == ClaimStatus.Pending ||
                    c.Status == ClaimStatus.ApprovedByCoordinator
                ),
                RejectedClaims = filteredClaims.Count(c =>
                    c.Status == ClaimStatus.RejectedByCoordinator ||
                    c.Status == ClaimStatus.RejectedByManager
                ),
                TotalAmount = filteredClaims
                    .Where(c => c.Status == ClaimStatus.ApprovedByManager)
                    .Sum(c => c.TotalAmount),
                Claims = filteredClaims.Select(c => new ClaimListViewModel
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
                    AdditionalNotes = c.AdditionalNotes
                }).ToList()
            };

            return report;
        }

        public async Task<InvoiceViewModel> GenerateInvoiceAsync(int claimId)
        {
            var claim = await _claimRepository.GetClaimWithDetailsAsync(claimId);
            if (claim == null)
                throw new InvalidOperationException("Claim not found");

            if (claim.Status != ClaimStatus.ApprovedByManager)
                throw new InvalidOperationException("Only approved claims can generate invoices");

            var approver = claim.ApprovedById != null
                ? await _userManager.FindByIdAsync(claim.ApprovedById)
                : null;

            return new InvoiceViewModel
            {
                ClaimId = claim.Id,
                InvoiceNumber = $"INV-{claim.Id:D6}-{DateTime.Now:yyyyMMdd}",
                InvoiceDate = DateTime.Now,
                LecturerName = claim.LecturerName,
                LecturerEmail = claim.Lecturer?.Email ?? "",
                HoursWorked = claim.HoursWorked,
                HourlyRate = claim.HourlyRate,
                TotalAmount = claim.TotalAmount,
                AdditionalNotes = claim.AdditionalNotes,
                ApprovedDate = claim.ApprovedDate,
                ApprovedByName = approver?.FullName ?? "System"
            };
        }

        public async Task<byte[]> ExportReportToPdfAsync(ReportViewModel report)
        {
            // Simple HTML to PDF conversion
            var html = GenerateReportHtml(report);
            return Encoding.UTF8.GetBytes(html);
        }

        public async Task<byte[]> ExportInvoiceToPdfAsync(InvoiceViewModel invoice)
        {
            var html = GenerateInvoiceHtml(invoice);
            return Encoding.UTF8.GetBytes(html);
        }

        private string GenerateReportHtml(ReportViewModel report)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html><html><head>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
            sb.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 20px; }");
            sb.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
            sb.AppendLine("th { background-color: #4a6cf7; color: white; }");
            sb.AppendLine(".summary { background: #f0f0f0; padding: 15px; border-radius: 5px; margin-bottom: 20px; }");
            sb.AppendLine("</style></head><body>");

            sb.AppendLine($"<h1>Claims Report</h1>");
            sb.AppendLine($"<p>Period: {report.StartDate:yyyy-MM-dd} to {report.EndDate:yyyy-MM-dd}</p>");

            sb.AppendLine("<div class='summary'>");
            sb.AppendLine($"<h3>Summary</h3>");
            sb.AppendLine($"<p><strong>Total Claims:</strong> {report.TotalClaims}</p>");
            sb.AppendLine($"<p><strong>Approved:</strong> {report.ApprovedClaims}</p>");
            sb.AppendLine($"<p><strong>Pending:</strong> {report.PendingClaims}</p>");
            sb.AppendLine($"<p><strong>Rejected:</strong> {report.RejectedClaims}</p>");
            sb.AppendLine($"<p><strong>Total Amount (Approved):</strong> R {report.TotalAmount:N2}</p>");
            sb.AppendLine("</div>");

            sb.AppendLine("<table>");
            sb.AppendLine("<thead><tr>");
            sb.AppendLine("<th>Claim ID</th><th>Lecturer</th><th>Date</th><th>Hours</th><th>Rate</th><th>Amount</th><th>Status</th>");
            sb.AppendLine("</tr></thead><tbody>");

            foreach (var claim in report.Claims)
            {
                sb.AppendLine($"<tr>");
                sb.AppendLine($"<td>{claim.Id}</td>");
                sb.AppendLine($"<td>{claim.LecturerName}</td>");
                sb.AppendLine($"<td>{claim.SubmittedDate:yyyy-MM-dd}</td>");
                sb.AppendLine($"<td>{claim.HoursWorked}</td>");
                sb.AppendLine($"<td>R {claim.HourlyRate:N2}</td>");
                sb.AppendLine($"<td>R {claim.TotalAmount:N2}</td>");
                sb.AppendLine($"<td>{claim.Status}</td>");
                sb.AppendLine($"</tr>");
            }

            sb.AppendLine("</tbody></table>");
            sb.AppendLine($"<p style='margin-top: 30px;'><em>Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}</em></p>");
            sb.AppendLine("</body></html>");

            return sb.ToString();
        }

        private string GenerateInvoiceHtml(InvoiceViewModel invoice)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html><html><head>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; margin: 40px; }");
            sb.AppendLine(".invoice-header { text-align: center; margin-bottom: 30px; }");
            sb.AppendLine(".invoice-details { margin: 20px 0; }");
            sb.AppendLine(".invoice-table { width: 100%; border-collapse: collapse; margin: 20px 0; }");
            sb.AppendLine(".invoice-table th, .invoice-table td { border: 1px solid #ddd; padding: 10px; }");
            sb.AppendLine(".invoice-table th { background-color: #4a6cf7; color: white; }");
            sb.AppendLine(".total-row { font-weight: bold; background-color: #f0f0f0; }");
            sb.AppendLine("</style></head><body>");

            sb.AppendLine("<div class='invoice-header'>");
            sb.AppendLine("<h1>INVOICE</h1>");
            sb.AppendLine($"<p><strong>Invoice Number:</strong> {invoice.InvoiceNumber}</p>");
            sb.AppendLine($"<p><strong>Date:</strong> {invoice.InvoiceDate:yyyy-MM-dd}</p>");
            sb.AppendLine("</div>");

            sb.AppendLine("<div class='invoice-details'>");
            sb.AppendLine("<h3>Bill To:</h3>");
            sb.AppendLine($"<p><strong>Lecturer:</strong> {invoice.LecturerName}</p>");
            sb.AppendLine($"<p><strong>Email:</strong> {invoice.LecturerEmail}</p>");
            sb.AppendLine($"<p><strong>Claim ID:</strong> {invoice.ClaimId}</p>");
            sb.AppendLine("</div>");

            sb.AppendLine("<table class='invoice-table'>");
            sb.AppendLine("<thead><tr>");
            sb.AppendLine("<th>Description</th><th>Quantity</th><th>Rate (ZAR)</th><th>Amount (ZAR)</th>");
            sb.AppendLine("</tr></thead><tbody>");

            sb.AppendLine("<tr>");
            sb.AppendLine($"<td>Lecturer Services - Contract Work</td>");
            sb.AppendLine($"<td>{invoice.HoursWorked} hours</td>");
            sb.AppendLine($"<td>R {invoice.HourlyRate:N2}</td>");
            sb.AppendLine($"<td>R {invoice.TotalAmount:N2}</td>");
            sb.AppendLine("</tr>");

            if (!string.IsNullOrEmpty(invoice.AdditionalNotes))
            {
                sb.AppendLine("<tr>");
                sb.AppendLine($"<td colspan='4'><strong>Notes:</strong> {invoice.AdditionalNotes}</td>");
                sb.AppendLine("</tr>");
            }

            sb.AppendLine("<tr class='total-row'>");
            sb.AppendLine($"<td colspan='3' style='text-align: right;'>TOTAL:</td>");
            sb.AppendLine($"<td>R {invoice.TotalAmount:N2}</td>");
            sb.AppendLine("</tr>");

            sb.AppendLine("</tbody></table>");

            sb.AppendLine("<div style='margin-top: 40px;'>");
            sb.AppendLine($"<p><strong>Approved By:</strong> {invoice.ApprovedByName}</p>");
            sb.AppendLine($"<p><strong>Approval Date:</strong> {invoice.ApprovedDate:yyyy-MM-dd}</p>");
            sb.AppendLine("</div>");

            sb.AppendLine($"<p style='margin-top: 50px; text-align: center;'><em>This is an automatically generated invoice</em></p>");
            sb.AppendLine("</body></html>");

            return sb.ToString();
        }
    }
}