using Microsoft.EntityFrameworkCore;
using our_group.Shared.Domain;

namespace our_group.Core.Domain.User.Services;

public class FriendshipService : IFriendshipService
{
    private readonly UserContext _context;

    public FriendshipService(UserContext context)
    {
        _context = context;
    }

    public async Task SendRequestAsync(string requesterId, string receiverId)
    {
        if (requesterId == receiverId)
            throw new InvalidOperationException("You cannot friend yourself.");

        var exists = await _context.Friendships.AnyAsync(f =>
            (f.RequesterId == requesterId && f.ReceiverId == receiverId) ||
            (f.RequesterId == receiverId && f.ReceiverId == requesterId));

        if (exists)
            throw new InvalidOperationException("A friendship already exists or is pending.");

        var freind = new Friendship
        {
            RequesterId = requesterId,
            ReceiverId = receiverId,
            Status = FriendshipStatus.Pending
        };

        _context.Friendships.Add(freind);
        await _context.SaveChangesAsync();
    }

    public async Task AcceptRequestAsync(string requesterId, string receiverId)
    {
        var friendship = await _context.Friendships.FirstOrDefaultAsync(f =>
            f.RequesterId == requesterId &&
            f.ReceiverId == receiverId &&
            f.Status == FriendshipStatus.Pending);

        if (friendship == null)
            throw new InvalidOperationException("No pending friendship request found.");

        friendship.Status = FriendshipStatus.Accepted;
        await _context.SaveChangesAsync();
    }

    public async Task DeclineRequestAsync(string requesterId, string receiverId)
    {
        var friendship = await _context.Friendships.FirstOrDefaultAsync(f =>
            f.RequesterId == requesterId &&
            f.ReceiverId == receiverId &&
            f.Status == FriendshipStatus.Pending);

        if (friendship == null)
            throw new InvalidOperationException("No pending friendship request found.");

        _context.Friendships.Remove(friendship);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveFriendAsync(string userId1, string userId2)
    {
        var friendship = await _context.Friendships.FirstOrDefaultAsync(f =>
            (f.RequesterId == userId1 && f.ReceiverId == userId2 && f.Status == FriendshipStatus.Accepted) ||
            (f.RequesterId == userId2 && f.ReceiverId == userId1 && f.Status == FriendshipStatus.Accepted));

        if (friendship == null)
            throw new InvalidOperationException("No friendship found to remove.");

        _context.Friendships.Remove(friendship);
        await _context.SaveChangesAsync();
    }

    public Task<int> GetFriendCountAsync(string userId)
    {
        return _context.Friendships.CountAsync(f =>
            (f.RequesterId == userId || f.ReceiverId == userId) &&
            f.Status == FriendshipStatus.Accepted);
    }

    public async Task<IEnumerable<UserAccount>> GetFriendsAsync(string userId)
    {
        var accepted = await _context.Friendships
            .Where(f => f.Status == FriendshipStatus.Accepted &&
                        (f.RequesterId == userId || f.ReceiverId == userId))
            .ToListAsync();

        var friendIds = accepted
            .Select(f => f.RequesterId == userId ? f.ReceiverId : f.RequesterId)
            .ToList();

        var users = await _context.Users
            .Where(u => friendIds.Contains(u.Id))
            .ToListAsync();

        return users.Select(u =>
            new UserAccount(
                new UserId(u.Id),
                new UserName(u.UserName),
                new EmailAddress(u.Email),
                u.CreatedAt));
    }

    public async Task<IEnumerable<Friendship>> GetIncomingRequestsAsync(string userId)
    {
        return await _context.Friendships
            .Where(f => f.ReceiverId == userId && f.Status == FriendshipStatus.Pending)
            .ToListAsync();
    }

    public async Task<IEnumerable<UserAccount>> SearchUsersAsync(string query, string excludeUserId)
    {
        var users = await _context.Users
            .Where(u =>
                u.Id != excludeUserId &&
                (u.UserName.Contains(query) || u.Email.Contains(query)))
            .ToListAsync();

        return users.Select(u =>
            new UserAccount(
                new UserId(u.Id),
                new UserName(u.UserName),
                new EmailAddress(u.Email),
                u.CreatedAt));
    }
}