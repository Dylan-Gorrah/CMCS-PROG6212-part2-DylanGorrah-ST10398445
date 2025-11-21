using System.ComponentModel.DataAnnotations;

namespace CMS_ASSIGNMENT.ViewModels
{
    public class ClaimViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Hours worked is required")]
        [Range(1, 200, ErrorMessage = "Hours worked must be between 1 and 200")]
        [Display(Name = "Hours Worked")]
        public decimal HoursWorked { get; set; }

        [Required(ErrorMessage = "Hourly rate is required")]
        [Range(0, 10000, ErrorMessage = "Hourly rate must be between 0 and 10,000")]
        [Display(Name = "Hourly Rate (ZAR)")]
        public decimal HourlyRate { get; set; }

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        [Display(Name = "Additional Notes")]
        public string? AdditionalNotes { get; set; }

        [Display(Name = "Supporting Document")]
        public IFormFile? Document { get; set; }
    }

    public class ClaimListViewModel
    {
        public int Id { get; set; }
        public string LecturerName { get; set; } = string.Empty;
        public string CoordinatorName { get; set; } = string.Empty;
        public DateTime SubmittedDate { get; set; }
        public decimal HoursWorked { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? DocumentFileName { get; set; }
        public string? AdditionalNotes { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime? RejectedDate { get; set; }
        public string? ApprovedByName { get; set; }
        public string? RejectedByName { get; set; }
        public bool IsFlaggedForReview { get; set; }
        public bool HasBlockingViolations { get; set; }
        public string? FlaggedReasons { get; set; }
    }

    public class ClaimDetailsViewModel
    {
        public int Id { get; set; }
        public string LecturerName { get; set; } = string.Empty;
        public string LecturerEmail { get; set; } = string.Empty;
        public string CoordinatorName { get; set; } = string.Empty;
        public DateTime SubmittedDate { get; set; }
        public decimal HoursWorked { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? AdditionalNotes { get; set; }
        public string? DocumentFileName { get; set; }
        public long? DocumentSize { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public DateTime? RejectedDate { get; set; }
        public string? ApprovedByName { get; set; }
        public string? RejectedByName { get; set; }
        public bool IsFlaggedForReview { get; set; }
        public bool HasBlockingViolations { get; set; }
        public string? FlaggedReasons { get; set; }
    }

    public class LecturerProfileViewModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }
    }

    public class ReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalClaims { get; set; }
        public int ApprovedClaims { get; set; }
        public int PendingClaims { get; set; }
        public int RejectedClaims { get; set; }
        public List<ClaimListViewModel> Claims { get; set; } = new();
    }

    public class InvoiceViewModel
    {
        public int ClaimId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public string LecturerName { get; set; } = string.Empty;
        public string LecturerEmail { get; set; } = string.Empty;
        public decimal HoursWorked { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal TotalAmount { get; set; }
        public string? AdditionalNotes { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string? ApprovedByName { get; set; }
    }
}