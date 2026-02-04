using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace our_group.Pages.Admin
{
    public class LogoutModel : PageModel
    {
        public async Task<IActionResult> OnPostAsync()
        {
            // Sign out all cookie schemes to avoid mixed auth state
            await HttpContext.SignOutAsync("CombinedCookies");
            await HttpContext.SignOutAsync("AdminCookie");
            await HttpContext.SignOutAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);

            // Explicitly delete admin cookie if still present
            if (Request.Cookies.ContainsKey("AdminAuth"))
            {
                Response.Cookies.Delete("AdminAuth", new Microsoft.AspNetCore.Http.CookieOptions { Path = "/" });
            }
            if (Request.Cookies.ContainsKey(".AspNetCore.Cookies"))
            {
                Response.Cookies.Delete(".AspNetCore.Cookies", new Microsoft.AspNetCore.Http.CookieOptions { Path = "/" });
            }
            // Clear JWT cookie (if the admin interacted with API)
            if (Request.Cookies.ContainsKey("AuthToken"))
            {
                Response.Cookies.Delete("AuthToken", new Microsoft.AspNetCore.Http.CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict
                });
            }
            return RedirectToPage("/Index");
        }
    }
}
