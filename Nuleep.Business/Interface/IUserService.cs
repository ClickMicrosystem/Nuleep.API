using Nuleep.Models;
using Nuleep.Models.Request;
using Nuleep.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Business.Interface
{
    public interface IUserService
    {
        Task<User> Authenticate(string username, string password);
        Task<SignupResponse?> Signup(SignupRequest request);
        Task<User> GetUserByUsername(string username);
        Task UpdateResetToken(int userId, string hashedToken, DateTime expiry);
        Task ClearResetToken(int userId);
        Task<User?> GetUserByResetToken(string hashedToken);
        Task UpdatePasswordAndClearToken(int userId, string newPasswordHash);
        Task<User?> UpdateIsProfileStatus(int id, bool isProfile);
        Task<User?> UpdateEmailVerifiedStatus(int id, bool isEmailVerified);
        Task RemoveEmployee(RemoveEmployeeRequest request);
        Task SendCompanyClaimEmail(CompanyClaimEmailRequest request);

    }
}
