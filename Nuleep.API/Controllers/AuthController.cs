using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Nuleep.Business.Interface;
using Nuleep.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Nuleep.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _config;

        public AuthController(IUserService userService, IConfiguration config)
        {
            _userService = userService;
            _config = config;
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
                    //data = user, // no need to paas user data
                    expTime = DateTime.UtcNow.AddDays(1).Ticks // Token expiration time
                });
            }
            else
            {
                return Unauthorized(new { success = false, data = "Invalid Credentials." });
            }

        }

        public class SignInRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
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
