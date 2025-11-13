using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CMS_ASSIGNMENT.Models
{
    public class Claim
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string LecturerId { get; set; } = string.Empty;

        [Required]
        public string LecturerName { get; set; } = string.Empty;

        [Required]
        public string CoordinatorId { get; set; } = string.Empty;

        [Required]
        public string CoordinatorName { get; set; } = string.Empty;

        [Required]
        [Range(1, 200)]
        public decimal HoursWorked { get; set; }

        [Required]
        [Range(0, 10000)]
        public decimal HourlyRate { get; set; }

        public decimal TotalAmount { get; set; }

        [StringLength(1000)]
        public string? AdditionalNotes { get; set; }

        public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ApprovedDate { get; set; }
        public DateTime? RejectedDate { get; set; }

        public string? ApprovedById { get; set; }
        public string? RejectedById { get; set; }

        [Required]
        public ClaimStatus Status { get; set; } = ClaimStatus.Pending;

        public string? DocumentFileName { get; set; }
        public string? DocumentFilePath { get; set; }

        [StringLength(100)]
        public string? DocumentContentType { get; set; }

        public long? DocumentSize { get; set; }

        // Navigation properties
        public virtual ApplicationUser? Lecturer { get; set; }
        public virtual ApplicationUser? Coordinator { get; set; }
        public virtual ApplicationUser? ApprovedBy { get; set; }
        public virtual ApplicationUser? RejectedBy { get; set; }
    }

    public enum ClaimStatus
    {
        Pending,
        ApprovedByCoordinator,
        RejectedByCoordinator,
        ApprovedByManager,
        RejectedByManager
    }
}