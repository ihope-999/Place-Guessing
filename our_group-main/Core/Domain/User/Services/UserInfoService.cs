using System;
using System.Threading.Tasks;
using our_group.Core.Domain.User.DTOs;

//using our_group.Core.Domain.Game;

namespace our_group.Core.Domain.User.Services;

public class UserInfoService : IUserInfoService/*, our_group.Core.Domain.Game.FakedData.IUserInfoService*/
{
    private readonly IPlayerProfileReader _reader;
    // ef user repository to get all users
    private readonly IUserRepository _userRepository;


    public UserInfoService(IPlayerProfileReader reader, IUserRepository userRepository)
    {
        _reader = reader;
        _userRepository = userRepository;
    }

    public async Task<PlayerInfoDto/*Player*/> GetPlayer(int userId)
    {
        var summary = await _reader.GetByPlayerIdAsync(userId);
        if (summary == null)
            throw new InvalidOperationException($"User with PlayerId '{userId}' not found.");

        // Map to the game Player aggregate
        return new PlayerInfoDto(userId, summary.UserName, 0);
    }

    public async Task<IEnumerable<UserAccount>> GetAllUsersAsync()
    {
        return await _userRepository.GetAllUsersAsync();
    }
}
