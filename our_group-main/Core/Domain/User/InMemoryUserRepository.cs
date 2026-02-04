using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using our_group.Shared.Domain;

namespace our_group.Core.Domain.User
{
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly ConcurrentDictionary<string, (UserAccount account, string passwordHash)> _users = new();

        public InMemoryUserRepository()
        {
            // Seed a demo user
            var id = new UserId("1");
            var name = new UserName("testuser");
            var email = new EmailAddress("test@example.com");
            var account = new UserAccount(id, name, email, System.DateTime.UtcNow);
            var hash = BCrypt.Net.BCrypt.HashPassword("Password123!");
            _users.TryAdd(name.Value.ToLowerInvariant(), (account, hash));
        }

        public Task<UserAccount?> AuthenticateAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return Task.FromResult<UserAccount?>(null);
            }

            if (_users.TryGetValue(username.ToLowerInvariant(), out var entry))
            {
                if (BCrypt.Net.BCrypt.Verify(password, entry.passwordHash))
                {
                    return Task.FromResult<UserAccount?>(entry.account);
                }
            }

            return Task.FromResult<UserAccount?>(null);
        }

        public Task<bool> ExistsByUsernameAsync(string username)
        {
            var exists = _users.ContainsKey(username.ToLowerInvariant());
            return Task.FromResult(exists);
        }

        public Task<bool> ExistsByEmailAsync(string email)
        {
            var exists = _users.Values.Any(v => v.account.Email.Value.Equals(email, System.StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(exists);
        }

        public Task<UserAccount> CreateAsync(string username, string email, string password)
        {
            var avatars = new[] { "🗺️","🏛️","🗽","🗼","🏔️","🏟️","🏝️","🏯","🕌","⛩️","🏙️","🧭","🧳","🚀","🎯" };
            var id = new UserId(System.Guid.NewGuid().ToString("N"));
            var name = new UserName(username);
            var mail = new EmailAddress(email);
            var account = new UserAccount(id, name, mail, System.DateTime.UtcNow);
            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            _users[username.ToLowerInvariant()] = (account, hash);
            return Task.FromResult(account);
        }

        public Task<IEnumerable<UserAccount>> GetAllUsersAsync()
        {
            IEnumerable<UserAccount> users = _users.Values.Select(v => v.account).ToList();
            return Task.FromResult(users);
        }
    }
}
