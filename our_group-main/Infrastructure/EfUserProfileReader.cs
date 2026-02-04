using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using our_group.Core.Domain.User;
using our_group.Infrastructure.Data;

namespace our_group.Infrastructure
{
    public class EfUserProfileReader : IUserProfileReader, our_group.Core.Domain.User.IPlayerProfileReader
    {
        private readonly UserContext _db;
        public EfUserProfileReader(UserContext db)
        {
            _db = db;
        }

        public async Task<UserSummary?> GetByIdAsync(string userId)
        {
            var u = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId);
            if (u == null) return null;
            var favs = (u.FavoriteRegions ?? string.Empty)
                .Split(',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries)
                .ToArray();
            return new UserSummary(u.Id, u.UserName, u.Email, u.CreatedAt, u.Avatar, null, favs);
        }

        public async Task<IReadOnlyDictionary<string, UserSummary>> GetByIdsAsync(IEnumerable<string> userIds)
        {
            var set = userIds.Distinct().ToArray();
            var users = await _db.Users.AsNoTracking().Where(x => set.Contains(x.Id)).ToListAsync();
            return users.ToDictionary(u => u.Id, u => new UserSummary(
                u.Id, u.UserName, u.Email, u.CreatedAt, u.Avatar, null,
                (u.FavoriteRegions ?? string.Empty)
                    .Split(',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries)
                    .ToArray()
            ));
        }

        public async Task<UserSummary?> GetByPlayerIdAsync(int playerId)
        {
            var u = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.PlayerId == playerId);
            if (u == null) return null;
            var favs = (u.FavoriteRegions ?? string.Empty)
                .Split(',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries)
                .ToArray();
            return new UserSummary(u.Id, u.UserName, u.Email, u.CreatedAt, u.Avatar, null, favs);
        }
    }
}

