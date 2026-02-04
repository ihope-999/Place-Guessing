using System.Threading.Tasks;
using our_group.Core.Domain.Game;
using our_group.Core.Domain.User.DTOs;

namespace our_group.Core.Domain.User.Services;

public interface IUserInfoService
{
    Task<PlayerInfoDto/*Player*/> GetPlayer(int userId);
   
   // get all users
    Task<IEnumerable<UserAccount>> GetAllUsersAsync();
    
}

