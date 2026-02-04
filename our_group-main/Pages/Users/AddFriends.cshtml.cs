using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using our_group.Core.Domain.User;
using our_group.Core.Domain.User.Services;

namespace our_group.Pages.Users;

public class FriendshipViewModel
{
    public int Id { get; set; }
    public string RequesterId { get; set; } = null!;
    public string RequesterUserName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class AddFriendsModel : PageModel
{
    private readonly IFriendshipService _friendshipService;
    private readonly UserContext _context;

    public AddFriendsModel(IFriendshipService friendshipService, UserContext context)
    {
        _friendshipService = friendshipService;
        _context = context;
    }

    public string CurrentUserId { get; set; }
    public string Query { get; set; }

    public IEnumerable<UserAccount> SearchResults { get; set; } = new List<UserAccount>();
    public IEnumerable<FriendshipViewModel> IncomingRequests { get; set; } = new List<FriendshipViewModel>();
    public int FriendCount { get; set; }
    public string RequesterUserName { get; set; } = null!;
    public string username { get; set; } = null!;

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

    public async Task<IActionResult> OnGet(string query)
    {
        CurrentUserId = await GetCurrentUserIdAsync();

        var incomingRequestsFromService = await _friendshipService.GetIncomingRequestsAsync(CurrentUserId);

        var requesterIds = incomingRequestsFromService
            .Select(r => r.RequesterId)
            .ToList();

        var users = await _context.Users
            .Where(u => requesterIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.UserName);

        IncomingRequests = incomingRequestsFromService.Select(r => new FriendshipViewModel
        {
            Id = r.Id,
            RequesterId = r.RequesterId,
            RequesterUserName = users.TryGetValue(r.RequesterId, out string? value) ? value : "Unknown",
            CreatedAt = r.CreatedAt
        })
        .ToList();

        FriendCount = await _friendshipService.GetFriendCountAsync(CurrentUserId);

        if (!string.IsNullOrWhiteSpace(query))
            SearchResults = await _friendshipService.SearchUsersAsync(query, CurrentUserId);

        return Page();
    }

    public async Task<IActionResult> OnPostSendRequest(string receiverId)
    {
        try
        {
            CurrentUserId = await GetCurrentUserIdAsync();
            Console.WriteLine("Sending friend request from " + CurrentUserId + " to " + receiverId);
            await _friendshipService.SendRequestAsync(CurrentUserId, receiverId);
            TempData["Message"] = "Friend request sent!";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAccept(string requesterId)
    {
        CurrentUserId = await GetCurrentUserIdAsync();
        await _friendshipService.AcceptRequestAsync(requesterId, CurrentUserId);
        return RedirectToPage("/Users/AddFriends");
    }

    public async Task<IActionResult> OnPostDecline(string requesterId)
    {
        CurrentUserId = await GetCurrentUserIdAsync(); ;
        await _friendshipService.DeclineRequestAsync(requesterId, CurrentUserId);
        return RedirectToPage("/Users/AddFriends");
    }
}
