using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Antiforgery;
using MediatR;
using our_group.Core.Domain.User;

namespace our_group.Pages.Users
{
    public class LogoutModel : PageModel
    {
        private readonly IMediator _mediator;

        public LogoutModel(IMediator mediator)
        {
            _mediator = mediator;
        }
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostAsync()
        {
            var account = new UserAccount(
                new our_group.Shared.Domain.UserId(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? ""),
                new our_group.Shared.Domain.UserName(User.Identity?.Name ?? ""),
                new our_group.Shared.Domain.EmailAddress(User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? string.Empty),
                System.DateTime.UtcNow // CreatedAt not available in cookie; value unused by listeners typically
            );
            await _mediator.Publish(new UserLoggedOut(account));
            // Sign out all cookie schemes to fully clear auth state
            await HttpContext.SignOutAsync("CombinedCookies");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync("AdminCookie");
            // Explicitly delete admin cookie if present
            if (Request.Cookies.ContainsKey("AdminAuth"))
            {
                Response.Cookies.Delete("AdminAuth", new Microsoft.AspNetCore.Http.CookieOptions { Path = "/" });
            }
            // Clear default auth cookie if present
            if (Request.Cookies.ContainsKey(".AspNetCore.Cookies"))
            {
                Response.Cookies.Delete(".AspNetCore.Cookies", new Microsoft.AspNetCore.Http.CookieOptions { Path = "/" });
            }
            // Clear JWT cookie used for API auth
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

