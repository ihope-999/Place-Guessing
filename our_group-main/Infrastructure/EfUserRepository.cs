using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using our_group.Core.Domain.User;
using our_group.Infrastructure.Data;
using our_group.Shared.Domain;

namespace our_group.Infrastructure
{
    public class EfUserRepository : IUserRepository
    {
        private readonly UserContext _db;
        public EfUserRepository(UserContext db)
        {
            _db = db;
        }

        public async Task<UserAccount?> AuthenticateAsync(string username, string password)
        {
            var norm = username.ToLowerInvariant();
            var entity = await _db.Users.FirstOrDefaultAsync(u => u.NormalizedUserName == norm);
            if (entity == null) return null;
            if (!BCrypt.Net.BCrypt.Verify(password, entity.PasswordHash)) return null;
            return ToDomain(entity);
        }

        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            var norm = username.ToLowerInvariant();
            return await _db.Users.AnyAsync(u => u.NormalizedUserName == norm);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            var norm = email.ToLowerInvariant();
            return await _db.Users.AnyAsync(u => u.NormalizedEmail == norm);
        }

        public async Task<UserAccount> CreateAsync(string username, string email, string password)
        {
            var avatars = new[] { "ðŸ—ºï¸", "ðŸ›ï¸", "ðŸ—½", "ðŸ—¼", "ðŸ”ï¸", "ðŸŸï¸", "ðŸï¸", "ðŸ¯", "ðŸ•Œ", "â›©ï¸", "ðŸ™ï¸", "ðŸ§­", "ðŸ§³", "ðŸš€", "ðŸŽ¯" };
            var entity = new User
            {
                Id = Guid.NewGuid().ToString("N"),
                UserName = username,
                NormalizedUserName = username.ToLowerInvariant(),
                Email = email,
                NormalizedEmail = email.ToLowerInvariant(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                CreatedAt = DateTime.UtcNow,
                Avatar = avatars[new Random().Next(avatars.Length)]
            };
            _db.Users.Add(entity);
            await _db.SaveChangesAsync();
            return ToDomain(entity);
        }

        private static UserAccount ToDomain(User e)
            => new UserAccount(new UserId(e.Id), new UserName(e.UserName), new EmailAddress(e.Email), e.CreatedAt);


                // get all users from database
        public async Task<IEnumerable<UserAccount>> GetAllUsersAsync()
        {
            var entities = await _db.Users.ToListAsync();
            return entities.Select(e => ToDomain(e));
        }
    }
    


   
}


