using Microsoft.AspNetCore.Identity;
using CMS_ASSIGNMENT.Models;

namespace CMS_ASSIGNMENT.Data
{
    public static class SeedData
    {
        public static async Task Initialize(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Create roles
            string[] roleNames = { "Lecturer", "Coordinator", "Manager", "HR" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create default admin user
            var adminUser = new ApplicationUser
            {
                UserName = "admin@university.com",
                Email = "admin@university.com",
                FirstName = "System",
                LastName = "Admin",
                Role = UserRole.Manager,
                EmailConfirmed = true
            };

            var adminExists = await userManager.FindByEmailAsync(adminUser.Email);
            if (adminExists == null)
            {
                var createAdmin = await userManager.CreateAsync(adminUser, "Admin123!");
                if (createAdmin.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Manager");
                }
            }

            // Create sample lecturer
            var lecturerUser = new ApplicationUser
            {
                UserName = "lecturer@university.com",
                Email = "lecturer@university.com",
                FirstName = "John",
                LastName = "Doe",
                Role = UserRole.Lecturer,
                EmailConfirmed = true
            };

            var lecturerExists = await userManager.FindByEmailAsync(lecturerUser.Email);
            if (lecturerExists == null)
            {
                var createLecturer = await userManager.CreateAsync(lecturerUser, "Lecturer123!");
                if (createLecturer.Succeeded)
                {
                    await userManager.AddToRoleAsync(lecturerUser, "Lecturer");
                }
            }

            // Create sample coordinator
            var coordinatorUser = new ApplicationUser
            {
                UserName = "coordinator@university.com",
                Email = "coordinator@university.com",
                FirstName = "Sarah",
                LastName = "Wilson",
                Role = UserRole.Coordinator,
                EmailConfirmed = true
            };

            var coordinatorExists = await userManager.FindByEmailAsync(coordinatorUser.Email);
            if (coordinatorExists == null)
            {
                var createCoordinator = await userManager.CreateAsync(coordinatorUser, "Coordinator123!");
                if (createCoordinator.Succeeded)
                {
                    await userManager.AddToRoleAsync(coordinatorUser, "Coordinator");
                }
            }

            // Create HR user
            var hrUser = new ApplicationUser
            {
                UserName = "hr@university.com",
                Email = "hr@university.com",
                FirstName = "Harper",
                LastName = "Reed",
                Role = UserRole.HR,
                EmailConfirmed = true
            };

            var hrExists = await userManager.FindByEmailAsync(hrUser.Email);
            if (hrExists == null)
            {
                var createHr = await userManager.CreateAsync(hrUser, "HrAdmin123!");
                if (createHr.Succeeded)
                {
                    await userManager.AddToRoleAsync(hrUser, "HR");
                }
            }
        }
    }
}