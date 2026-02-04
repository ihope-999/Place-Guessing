using System;

namespace our_group.Core.Domain.User
{
    public class User
    {
        public int PlayerId { get; set; }
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string UserName { get; set; } = string.Empty;
        public string NormalizedUserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NormalizedEmail { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        // Optional profile fields
        public string? Avatar { get; set; }
        // Comma-separated list of region slugs (e.g., "europe,asia")
        public string? FavoriteRegions { get; set; }

        public int Rank { get; set; } = 0;
    }
}
