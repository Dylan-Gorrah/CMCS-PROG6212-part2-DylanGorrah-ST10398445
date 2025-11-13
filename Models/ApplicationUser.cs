using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CMS_ASSIGNMENT.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        public string FullName => $"{FirstName} {LastName}";

        [Required]
        public UserRole Role { get; set; }

        // Navigation properties - make them nullable
        public virtual ICollection<Claim>? SubmittedClaims { get; set; }
        public virtual ICollection<Claim>? CoordinatedClaims { get; set; }
    }

    public enum UserRole
    {
        Lecturer,
        Coordinator,
        Manager
    }
}