using System.Threading.Tasks;

namespace our_group.Core.Domain.Admin
{
    public interface IAdminRepository
    {
        Task<bool> ExistsAsync();
        Task CreateAsync(string username, string password, bool mustChangePassword = true);
        Task<(string Id, string UserName, bool MustChangePassword)?> AuthenticateAsync(string username, string password);
        Task<bool> ChangePasswordAsync(string adminId, string newPassword);
        Task<bool> GetMustChangePasswordAsync(string adminId);
    }
}

