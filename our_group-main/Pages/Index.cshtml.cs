using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace our_group.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }


    public IActionResult OnGet()
    {
        var nameId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(nameId))
        {
            return RedirectToPage("/Users/Login");
        }

        if (!int.TryParse(nameId, out var playerId))
        {
            return RedirectToPage("/Users/Login");
        }

        return Page();
    }
}
