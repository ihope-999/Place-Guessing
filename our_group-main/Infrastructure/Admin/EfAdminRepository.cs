using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using our_group.Core.Domain.Admin;
using our_group.Infrastructure.Data;

namespace our_group.Infrastructure.Admin
{
    public class EfAdminRepository : IAdminRepository
    {
        private readonly UserContext _db;
        public EfAdminRepository(UserContext db)
        {
            _db = db;
        }

        public async Task<bool> ExistsAsync()
        {
            return await _db.Admins.AnyAsync();
        }

        public async Task CreateAsync(string username, string password, bool mustChangePassword = true)
        {
            var entity = new our_group.Core.Domain.Admin.Admin
            {
                Id = Guid.NewGuid().ToString("N"),
                UserName = username,
                NormalizedUserName = username.ToLowerInvariant(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                MustChangePassword = mustChangePassword,
                CreatedAt = DateTime.UtcNow,
            };
            _db.Admins.Add(entity);
            await _db.SaveChangesAsync();
        }

        public async Task<(string Id, string UserName, bool MustChangePassword)?> AuthenticateAsync(string username, string password)
        {
            var norm = username.ToLowerInvariant();
            var admin = await _db.Admins.FirstOrDefaultAsync(a => a.NormalizedUserName == norm);
            if (admin == null) return null;
            if (!BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash)) return null;
            admin.LastLoginAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return (admin.Id, admin.UserName, admin.MustChangePassword);
        }

        public async Task<bool> ChangePasswordAsync(string adminId, string newPassword)
        {
            var admin = await _db.Admins.FirstOrDefaultAsync(a => a.Id == adminId);
            if (admin == null) return false;
            admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            admin.MustChangePassword = false;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> GetMustChangePasswordAsync(string adminId)
        {
            var admin = await _db.Admins.AsNoTracking().FirstOrDefaultAsync(a => a.Id == adminId);
            return admin?.MustChangePassword ?? false;
        }
    }
}


