using System;

namespace our_group.Core.Domain.Admin
{
    public class Admin
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string UserName { get; set; } = string.Empty;
        public string NormalizedUserName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public bool MustChangePassword { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
    }
}

