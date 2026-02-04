using System.Threading.Tasks;
using System.Collections.Generic;

namespace our_group.Core.Domain.User
{
    public interface IUserRepository
    {
        Task<UserAccount?> AuthenticateAsync(string username, string password);
        Task<bool> ExistsByUsernameAsync(string username);
        Task<bool> ExistsByEmailAsync(string email);
        Task<UserAccount> CreateAsync(string username, string email, string password);
        // Retrieve all users from the database
        Task<IEnumerable<UserAccount>> GetAllUsersAsync();
    }
}
