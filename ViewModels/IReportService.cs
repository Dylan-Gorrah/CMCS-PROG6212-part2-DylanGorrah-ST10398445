using CMS_ASSIGNMENT.ViewModels;

namespace CMS_ASSIGNMENT.Interfaces
{
    public interface IReportService
    {
        Task<ReportViewModel> GenerateMonthlyReportAsync(int year, int month);
        Task<ReportViewModel> GenerateDateRangeReportAsync(DateTime startDate, DateTime endDate);
        Task<InvoiceViewModel> GenerateInvoiceAsync(int claimId);
        Task<byte[]> ExportReportToPdfAsync(ReportViewModel report);
        Task<byte[]> ExportInvoiceToPdfAsync(InvoiceViewModel invoice);
    }
}