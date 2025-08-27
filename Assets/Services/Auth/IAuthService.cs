using iBMSApp.Shared;
using iBMSApp.Utility;
using System.Threading.Tasks;

namespace iBMSApp.Services
{
    public interface IAuthService
    {
        Task<string> LoginAsync(UserInfo userInfo);
        Task AddAuthenticationHeader();
        Task LogoutAsync();
        //Task<string> getUserName();
        //Task<string> getUserID();
        //Task<List<string>> getRoles();
        Task<UserInfo> GetUserData();
        Task<bool> isInRole(UserRole role);

        Task<ApiResponse<string>> ChangePassword(string oldPassword, string newPassword, string confirmPassword);
    }
}
