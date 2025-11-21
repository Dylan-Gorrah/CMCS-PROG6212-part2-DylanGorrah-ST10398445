using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using CMS_ASSIGNMENT.Models;
using Microsoft.AspNetCore.Identity;

namespace CMS_ASSIGNMENT.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Redirect authenticated users to their respective dashboards
            if (User?.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    return user.Role switch
                    {
                        UserRole.Lecturer => RedirectToAction("Index", "Lecturer"),
                        UserRole.Coordinator => RedirectToAction("Index", "Coordinator"),
                        UserRole.Manager => RedirectToAction("Index", "Manager"),
                        UserRole.HR => RedirectToPage("/Index", new { area = "HR" }),
                        _ => View()
                    };
                }
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}