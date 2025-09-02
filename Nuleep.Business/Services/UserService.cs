using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DocumentFormat.OpenXml.Spreadsheet;
using MailChimp.Net;
using MailChimp.Net.Interfaces;
using MailChimp.Net.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Nuleep.Business.Interface;
using Nuleep.Data;
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
        private readonly IProfileRepository _profileRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IConfiguration _config;
        private readonly string _listId;
        private readonly IMailChimpManager _mailChimpManager;

        public UserService(IUserRepository userRepo, IConfiguration config, IProfileRepository profileRepository, IOrganizationRepository organizationRepository)
        {
            _userRepo = userRepo;
            _config = config;
            _profileRepository = profileRepository;
            _organizationRepository = organizationRepository;
            var apiKey = config["MailChimp:ApiKey"];
            var serverPrefix = config["MailChimp:ServerPrefix"];
            _listId = config["MailChimp:ListId"];

            _mailChimpManager = new MailChimpManager(apiKey);
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
        //public async Task<AdminSigninResponse> AdminSignin(ClaimsPrincipal user)
        //{
        //    if (user == null || !user.Identity.IsAuthenticated)
        //        throw new UnauthorizedAccessException("Invalid credentials");

        //    var role = user.FindFirst(ClaimTypes.Role)?.Value;
        //    var email = user.FindFirst(ClaimTypes.Email)?.Value;

        //    if (role != "admin")
        //        throw new UnauthorizedAccessException("Access denied");

        //    return new AdminSigninResponse
        //    {
        //        Email = email,
        //        Role = role,
        //        Token = ""
        //    };
        //}
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

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 10);

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
        public async Task UpdateGoogleId(int userId, string googleId)
        {
            await _userRepo.UpdateGoogleId(userId, googleId);
        }

        public async Task<int> CreateUser(User user)
        {
            return await _userRepo.CreateUser(user);
        }

        public async Task<User> ResetPasswordForOwnership(string resetToken, ResetOwnershipRequest dto)
        {
            using var sha = SHA256.Create();
            var tokenHash = BitConverter.ToString(
                sha.ComputeHash(Encoding.UTF8.GetBytes(resetToken))
            ).Replace("-", "").ToLower();

            var user = await _userRepo.GetUserByResetToken(tokenHash);
            if (user == null) throw new Exception("Invalid Token");

            var decodedData = Encoding.ASCII.GetString(Convert.FromBase64String(dto.Data));
            var dsc = JsonSerializer.Deserialize<Dictionary<string, string>>(decodedData);

            user.Email = dsc["newEmail"];
            user.FirstName = dsc["firstName"];
            user.LastName = dsc["lastName"];

            var profile = await _profileRepository.GetExistingProfileByUserAsync(user.Id.ToString());
            profile.Email = dsc["newEmail"];
            profile.FirstName = dsc["firstName"];
            profile.LastName = dsc["lastName"];

            await _profileRepository.UpdateProfile(profile);

            var orgId = int.Parse(Encoding.ASCII.GetString(Convert.FromBase64String(dto.OrgId)));
            Organization org = new Organization()
            {
                Email = user.Email,
                SendOwnership = true,
                Verified = true,
                Id = orgId,
            };

            await _userRepo.UpdateOrganization(org);

            // set new password
            user.Password = dto.Password;
            user.ResetPasswordToken = null;
            user.ResetPasswordExpire = null;
            user.IsProfile = true;
            user.ValidateEmail = true;

            await _userRepo.UpdateUser(user);

            return user;
        }

        public async Task<object> AddMailChimpMember(string email, string firstName, string lastName)
        {
            try
            {
                var member = new MailChimp.Net.Models.Member
                {
                    EmailAddress = email,
                    StatusIfNew = Status.Subscribed,
                    MergeFields = new Dictionary<string, object>
                    {
                        { "FNAME", firstName },
                        { "LNAME", lastName }
                    }
                };

                await _mailChimpManager.Members.AddOrUpdateAsync(_listId, member);

                return new { StatusCode = 200, Success = "Subscribed successfully." };
            }
            catch (Exception ex)
            {
                return new { StatusCode = 500, Message = $"Unable to subscribe: {ex.Message}" };
            }
        }
    }
}
