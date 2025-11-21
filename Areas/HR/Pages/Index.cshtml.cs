using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CMS_ASSIGNMENT.Areas.HR.Pages
{
    [Authorize(Roles = "HR")]
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
