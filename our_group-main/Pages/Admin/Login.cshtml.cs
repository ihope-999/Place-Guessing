using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using our_group.Core.Domain.Admin;

namespace our_group.Pages.Admin
{
    public class LoginModel : PageModel
    {
        private readonly IAdminRepository _repo;
        public LoginModel(IAdminRepository repo)
        {
            _repo = repo;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            public string Username { get; set; } = string.Empty;
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;
        }

        public void OnGet(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            if (!ModelState.IsValid) return Page();

            var auth = await _repo.AuthenticateAsync(Input.Username, Input.Password);
            if (auth is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid admin credentials.");
                return Page();
            }

            var (id, userName, mustChange) = auth.Value;
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, id),
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.Role, "Admin")
            };
            var identity = new ClaimsIdentity(claims, "AdminCookie");
            var props = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = System.DateTimeOffset.UtcNow.AddHours(8)
            };
            await HttpContext.SignInAsync("AdminCookie", new ClaimsPrincipal(identity), props);

            if (mustChange)
            {
                return RedirectToPage("/Admin/ChangePassword");
            }

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToPage("/Admin/Dashboard");
        }
    }
}

