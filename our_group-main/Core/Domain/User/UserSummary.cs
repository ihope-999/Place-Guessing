using System;

namespace our_group.Core.Domain.User
{
    public class UserSummary
    {
        public string Id { get; }
        public string UserName { get; }
        public string Email { get; }
        public DateTime CreatedAt { get; }
        public string? Avatar { get; }
        public int? Rating { get; }
        public IReadOnlyList<string> FavoriteRegions { get; }

        public UserSummary(string id, string userName, string email, DateTime createdAt, string? avatar = null, int? rating = null, IReadOnlyList<string>? favoriteRegions = null)
        {
            Id = id;
            UserName = userName;
            Email = email;
            CreatedAt = createdAt;
            Avatar = avatar;
            Rating = rating;
            FavoriteRegions = favoriteRegions ?? Array.Empty<string>();
        }
    }
}
