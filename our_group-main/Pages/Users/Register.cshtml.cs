using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using our_group.Core.Domain.User;
using System.Linq;

namespace our_group.Pages.Users
{
    public class RegisterModel : PageModel
    {
        private readonly IMediator _mediator;
        public RegisterModel(IMediator mediator)
        {
            _mediator = mediator;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            public string Username { get; set; } = string.Empty;
            [Required]
            [EmailAddress]
            public string Email { get; set; } = string.Empty;
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;
        }

        public void OnGet() {}

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            try
            {
                var result = await _mediator.Send(new Register(Input.Username, Input.Email, Input.Password));

                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, result.Account.Id.Value),
                    new Claim(ClaimTypes.Name, result.Account.Name.Value),
                    new Claim(ClaimTypes.Email, result.Account.Email.Value)
                };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                // JWT not used; rely solely on cookie auth

                return RedirectToPage("/Users/Login");
            }
            catch (System.InvalidOperationException ex)
            {
                var msg = ex.Message ?? "Registration failed";
                if (msg.Contains("Username", System.StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("Input.Username", msg);
                }
                else if (msg.Contains("Email", System.StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("Input.Email", msg);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, msg);
                }
                return Page();
            }
        }
    }
}

