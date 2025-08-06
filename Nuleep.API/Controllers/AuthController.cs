using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.IdentityModel.Tokens;
using Nuleep.Business.Interface;
using Nuleep.Business.Services;
using Nuleep.Models;
using Nuleep.Models.Request;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static Nuleep.API.Controllers.AuthController;

namespace Nuleep.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly EmailService _emailService;
        private readonly IConfiguration _config;

        public AuthController(IUserService userService, IConfiguration config, EmailService emailService)
        {
            _userService = userService;
            _config = config;
            _emailService = emailService;
        }

        [HttpPost("singleSignin")]
        public async Task<IActionResult> Login([FromBody] SignInRequest request)
        {

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 10);

            var user = await _userService.Authenticate(request.Email, request.Password);


            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                return Unauthorized(new { success = false, data = "Invalid Credentials." });

            if (!user.ValidateEmail || (user.Role != "jobSeeker" && user.Role != "recruiter"))
                return Unauthorized("Invalid user status.");

            if(user.ValidateEmail && (user.Role == "jobSeeker" || user.Role == "recruiter"))
            {
                user.Password = null; // Assuming Password is a property in your User model

                return Ok(new
                {
                    token = GenerateToken(user),
                    data = user, // no need to paas user data
                    expTime = DateTime.UtcNow.AddDays(1).Ticks // Token expiration time
                });
            }
            else
            {
                return Unauthorized(new { success = false, data = "Invalid Credentials." });
            }

        }
        
        [HttpPost("signup")]
        [AllowAnonymous]
        public async Task<IActionResult> Signup([FromBody] SignupRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return UnprocessableEntity(new { data = "You must provide email and password" });
            }

            var result = await _userService.Signup(request);
            if (result == null)
            {
                return UnprocessableEntity(new { data = "Email in use" });
            }
            result.Token = GenerateToken(result.Data);
            return Ok(result);
        }

        [HttpPost("forgotpassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var user = await _userService.GetUserByUsername(request.Email);
            if (user == null)
                return NotFound(new { error = "No user with that email" });

            var (token, hashedToken, expiry) = GenerateResetToken();

            await _userService.UpdateResetToken(user.Id, hashedToken, expiry);

            var resetUrl = $"{_config["FrontendUrl"]}/forgotpassword/{token}";

            var emailSent = await _emailService.SendResetEmail(user.Email, resetUrl);

            if (!emailSent)
            {
                await _userService.ClearResetToken(user.Id);
                return StatusCode(500, new { error = "Email could not be sent" });
            }

            return Ok(new { success = true });
        }

        [HttpPost("resetPassword/{resetToken}")]
        public async Task<IActionResult> ResetPassword(string resetToken, [FromBody] SetPasswordRequest request)
        {
            // Hash the token
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(resetToken);
            var hash = sha.ComputeHash(bytes);
            var hashedToken = BitConverter.ToString(hash).Replace("-", "").ToLower();

            // Find user by token
            var user = await _userService.GetUserByResetToken(hashedToken);
            if (user == null)
                return BadRequest(new { error = "Invalid or expired token" });

            // Hash the new password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Update password and clear token
            await _userService.UpdatePasswordAndClearToken(user.Id, hashedPassword);

            // Return JWT token
            var token = GenerateToken(user);

            return Ok(new { token });
        }

        [HttpPost("profileStatus")]
        public async Task<IActionResult> UpdateProfileStatus([FromBody] ProfileStatusRequest request)
        {
            var updatedUser = await _userService.UpdateIsProfileStatus(request.Id, request.Data.IsProfile);

            if (updatedUser == null)
                return NotFound(new { success = false, message = "User not found" });

            return Ok(new { success = true, data = updatedUser });
        }

        [HttpPost("verifyEmailStatus")]
        public async Task<IActionResult> VerifyEmailStatus([FromBody] ProfileStatusRequest request)
        {
            var updatedUser = await _userService.UpdateEmailVerifiedStatus(request.Id, request.Data.ValidateEmail);

            if (updatedUser == null)
                return NotFound(new { success = false, message = "User not found" });

            return Ok(new { success = true, data = updatedUser });
        }

        [HttpPost("removeEmployee")]
        public async Task<IActionResult> RemoveEmployee([FromBody] RemoveEmployeeRequest request)
        {
            try
            {
                await _userService.RemoveEmployee(request);
                return Ok(new { success = true, data = "Transfer successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("claimRequestEmail")]
        public async Task<IActionResult> SendCompanyClaimEmail([FromBody] CompanyClaimEmailRequest request)
        {
            try
            {
                await _userService.SendCompanyClaimEmail(request);
                return Ok(new { success = true });
            }
            catch (Exception)
            {
                return StatusCode(500, new { error = "Email could not be sent" });
            }
        }

        public static (string Token, string HashedToken, DateTime Expiry) GenerateResetToken()
        {
            var tokenBytes = RandomNumberGenerator.GetBytes(20);
            var token = Convert.ToHexString(tokenBytes).ToLower(); // plain token for URL

            using var sha256 = SHA256.Create();
            var hashedToken = BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(token))).Replace("-", "").ToLower();

            var expiry = DateTime.UtcNow.AddMinutes(10);

            return (token, hashedToken, expiry);
        }

        private string GenerateToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            //var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"])
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }

}
