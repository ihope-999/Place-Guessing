using System.Threading.Tasks;

namespace our_group.Core.Domain.User;

public interface IPlayerProfileReader
{
    Task<UserSummary?> GetByPlayerIdAsync(int playerId);
}

