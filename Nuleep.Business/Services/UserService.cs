using Microsoft.Extensions.Configuration;
using Nuleep.Business.Interface;
using Nuleep.Data.Interface;
using Nuleep.Data.Repository;
using Nuleep.Models;
using Nuleep.Models.Request;
using Nuleep.Models.Response;
using SendGrid.Helpers.Mail;

namespace Nuleep.Business.Services
{
    public class UserService:IUserService
    {
        
        private readonly IUserRepository _userRepo;
        private readonly IConfiguration _config;

        public UserService(IUserRepository userRepo, IConfiguration config)
        {
            _userRepo = userRepo;
            _config = config;
        }

        public async Task<User> Authenticate(string username, string password)
        {
            return await _userRepo.GetUserAsync(username, password);
        }

        public async Task<User> GetUserByUsername(string username)
        {
            return await _userRepo.GetUserByUsername(username);
        }
        public async Task UpdateResetToken(int userId, string hashedToken, DateTime expiry)
        {
            await _userRepo.UpdateResetToken(userId, hashedToken, expiry);
        }
        public async Task ClearResetToken(int userId)
        {
            await _userRepo.ClearResetToken(userId);
        }
        public async Task<User?> GetUserByResetToken(string hashedToken)
        {
           return await _userRepo.GetUserByResetToken(hashedToken);
        }
        public async Task UpdatePasswordAndClearToken(int userId, string newPasswordHash)
        {
           await _userRepo.UpdatePasswordAndClearToken(userId, newPasswordHash);
        }
        public async Task RemoveEmployee(RemoveEmployeeRequest request)
        {
           await _userRepo.RemoveEmployee(request);
        }
        public async Task<User?> UpdateIsProfileStatus(int id, bool isProfile)
        {
           return await _userRepo.UpdateIsProfileStatus(id, isProfile);
        }
        public async Task<User?> UpdateEmailVerifiedStatus(int id, bool isEmailVerified)
        {
           return await _userRepo.UpdateIsProfileStatus(id, isEmailVerified);
        }

        public async Task SendCompanyClaimEmail(CompanyClaimEmailRequest request)
        {
            var frontendUrl = _config["FrontendUrl"];
            var resetUrl = $"{frontendUrl}/admin/organization/{request.OrgId}/ownerShip";

            var message = $"You are receiving this email because you (or someone else) has requested the reset of a password. Please use this link to reset your password: {resetUrl}";

            var html = $@"
            <p>{request.FName} {request.LName} with an email address of {request.ReqEmail} claimed {request.CompName}.</p>
            <a href=""{resetUrl}"" style=""font-size: 20px"">click here</a>
        ";

            var client = new SendGrid.SendGridClient(_config["SendGrid:ApiKey"]);
            var from = new EmailAddress("claimcompany@nuleep.com", "Nuleep Claim");
            var to = new EmailAddress("claimcompany@nuleep.com");
            var subject = $"Claim for {request.CompName}";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, message, html);

            var response = await client.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Email could not be sent");
            }
        }
        public async Task<SignupResponse?> Signup(SignupRequest request)
        {
            var existingUser = await _userRepo.GetUserByUsername(request.Email);
            if (existingUser != null) return null;

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var newUser = new User
            {
                Email = request.Email,
                Password = hashedPassword,
                Role = request.UserType
            };

            var userId = await _userRepo.CreateUser(newUser);
            newUser.Id = userId;

            return new SignupResponse
            {
                Token = "",
                Data = new User
                {
                    Id = newUser.Id,
                    Email = newUser.Email,
                    Role = newUser.Role
                },
                ExpTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 1000 * 86400
            };
        }
    }
}
