using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using our_group.Core.Domain.User;
using our_group.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace our_group.Pages.Users
{
    public class LoginModel : PageModel
    {
        private readonly IMediator _mediator;
        private readonly UserContext _db;

        public LoginModel(IMediator mediator, UserContext db)
        {
            _mediator = mediator;
            _db = db;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            [Display(Name = "Username")]
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
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _mediator.Send(new Core.Domain.User.Login(Input.Username, Input.Password));
            var account = result.Account;
            if (account == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return Page();
            }

            // Load numeric PlayerId for claim-based integration with game domain
            var normUser = Input.Username.ToLowerInvariant();
            var entity = await _db.Users.FirstOrDefaultAsync(u => u.NormalizedUserName == normUser);
            if (entity == null)
            {
                // Safety: fall back to profile Id if user row not found (should not happen)
                entity = new our_group.Core.Domain.User.User { PlayerId = 0, Id = account.Id.Value, UserName = account.Name.Value, NormalizedUserName = normUser, Email = account.Email.Value, NormalizedEmail = account.Email.Value.ToLowerInvariant() };
            }
            if (entity.PlayerId == 0)
            {
                var currentMax = (await _db.Users.MaxAsync(u => (int?)u.PlayerId)) ?? 0;
                entity.PlayerId = currentMax + 1;
                await _db.SaveChangesAsync();
            }
            var playerIdValue = entity.PlayerId.ToString();

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, playerIdValue),
                new Claim(ClaimTypes.Name, account.Name.Value),
                new Claim(ClaimTypes.Email, account.Email.Value)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // JWT not used; rely solely on cookie auth

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToPage("/Users/Profile");
        }
    }
}


