using System.ComponentModel.DataAnnotations;
using CMS_ASSIGNMENT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CMS_ASSIGNMENT.Areas.HR.Pages.Lecturers
{
    [Authorize(Roles = "HR")]
    public class EditModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public EditModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["ErrorMessage"] = "Invalid lecturer identifier.";
                return RedirectToPage("./Index");
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null || user.Role != UserRole.Lecturer)
            {
                TempData["ErrorMessage"] = "Unable to locate the lecturer.";
                return RedirectToPage("./Index");
            }

            Input = new InputModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var lecturer = await _userManager.FindByIdAsync(Input.Id);
            if (lecturer == null || lecturer.Role != UserRole.Lecturer)
            {
                TempData["ErrorMessage"] = "Unable to locate the lecturer.";
                return RedirectToPage("./Index");
            }

            // Update basic fields
            lecturer.FirstName = Input.FirstName;
            lecturer.LastName = Input.LastName;
            lecturer.PhoneNumber = Input.PhoneNumber;
            lecturer.Email = Input.Email;
            lecturer.UserName = Input.Email;

            var updateResult = await _userManager.UpdateAsync(lecturer);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }

            if (!string.IsNullOrWhiteSpace(Input.NewPassword))
            {
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(lecturer);
                var passwordResult = await _userManager.ResetPasswordAsync(lecturer, resetToken, Input.NewPassword);
                if (!passwordResult.Succeeded)
                {
                    foreach (var error in passwordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }
            }

            TempData["SuccessMessage"] = $"Lecturer {lecturer.FullName} updated.";
            return RedirectToPage("./Index");
        }

        public class InputModel
        {
            [Required]
            public string Id { get; set; } = string.Empty;

            [Required]
            [StringLength(100)]
            public string FirstName { get; set; } = string.Empty;

            [Required]
            [StringLength(100)]
            public string LastName { get; set; } = string.Empty;

            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Phone]
            [Display(Name = "Phone Number")]
            public string? PhoneNumber { get; set; }

            [DataType(DataType.Password)]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "The {0} must be at least {2} characters long.")]
            public string? NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm Password")]
            [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
            public string? ConfirmPassword { get; set; }
        }
    }
}
