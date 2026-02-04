using System;

namespace our_group.Core.Domain.User
{
    public class Friendship
    {
        public int Id { get; set; }

        public string RequesterId { get; set; } = null!;
        public string ReceiverId { get; set; } = null!;

        public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
