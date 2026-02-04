using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using our_group.Core.Domain.Admin;

namespace our_group.Pages.Admin
{
    public class ChangePasswordModel : PageModel
    {
        private readonly IAdminRepository _repo;
        public ChangePasswordModel(IAdminRepository repo)
        {
            _repo = repo;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            [MinLength(8)]
            public string NewPassword { get; set; } = string.Empty;
            [Required]
            [DataType(DataType.Password)]
            [Compare("NewPassword")] 
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGet()
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(adminId)) return RedirectToPage("/Admin/Login");
            var must = await _repo.GetMustChangePasswordAsync(adminId);
            if (!must) return RedirectToPage("/Admin/Dashboard");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(adminId)) return RedirectToPage("/Admin/Login");
            var ok = await _repo.ChangePasswordAsync(adminId, Input.NewPassword);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, "Failed to change password.");
                return Page();
            }
            return RedirectToPage("/Admin/Dashboard");
        }
    }
}


