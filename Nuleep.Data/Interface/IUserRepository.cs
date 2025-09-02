using Nuleep.Models;
using Nuleep.Models.Request;

namespace Nuleep.Data.Interface
{
    public interface IUserRepository
    {
        Task<User> GetUserAsync(string username, string password);
        Task<User> GetUserByUsername(string username);
        Task<int> CreateUser(User user);
        Task UpdateResetToken(int userId, string hashedToken, DateTime expiry);
        Task ClearResetToken(int userId);
        Task<User?> GetUserByResetToken(string hashedToken);
        Task UpdatePasswordAndClearToken(int userId, string newPasswordHash);
        Task<User?> UpdateIsProfileStatus(int id, bool isProfile);
        Task<User?> UpdateEmailVerifiedStatus(int id, bool isEmailVerified);
        Task RemoveEmployee(RemoveEmployeeRequest request);
        Task UpdateGoogleId(int userId, string googleId);
        Task UpdateOrganization(Organization org);
        Task UpdateUser(User user);

    }
}
