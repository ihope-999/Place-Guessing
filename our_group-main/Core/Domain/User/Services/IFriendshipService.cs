using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace our_group.Core.Domain.User.Services;

public interface IFriendshipService
{
    Task SendRequestAsync(string requesterId, string receiverId);
    Task AcceptRequestAsync(string requesterId, string receiverId);
    Task DeclineRequestAsync(string requesterId, string receiverId);
    Task RemoveFriendAsync(string userId1, string userId2);
    Task<int> GetFriendCountAsync(string userId);
    Task<IEnumerable<UserAccount>> GetFriendsAsync(string userId);
    Task<IEnumerable<Friendship>> GetIncomingRequestsAsync(string userId);
    Task<IEnumerable<UserAccount>> SearchUsersAsync(string query, string excludeUserId);
}
