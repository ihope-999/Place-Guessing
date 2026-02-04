using System.Collections.Generic;
using System.Threading.Tasks;

namespace our_group.Core.Domain.User
{
    public interface IUserProfileReader
    {
        Task<UserSummary?> GetByIdAsync(string userId);
        Task<IReadOnlyDictionary<string, UserSummary>> GetByIdsAsync(IEnumerable<string> userIds);
    }
}

