using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using our_group.Core.Domain.User;
using our_group.Core.Domain.User.Services;

namespace our_group.Pages.Users;

public class FriendsModel : PageModel
{
    private readonly IFriendshipService _friendshipService;
    private readonly UserContext _context;

    public FriendsModel(IFriendshipService friendshipService, UserContext context)
    {
        _friendshipService = friendshipService;
        _context = context;
    }

    public IEnumerable<UserAccount> Friends { get; set; } = new List<UserAccount>();

    private async Task<string> GetCurrentUserIdAsync()
    {
        var playerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(playerIdClaim))
            throw new Exception("Current user not found in claims.");

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.PlayerId.ToString() == playerIdClaim);

        if (user == null)
            throw new Exception("Current user not found in the database.");

        return user.Id;
    }

    public async Task<IActionResult> OnGet()
    {
        var userId = await GetCurrentUserIdAsync();
        Friends = await _friendshipService.GetFriendsAsync(userId);
        return Page();
    }

    public async Task<IActionResult> OnPostRemove(string friendId)
    {
        var userId = await GetCurrentUserIdAsync();
        await _friendshipService.RemoveFriendAsync(userId, friendId);
        return RedirectToPage("/Users/Friends");
    }
}
