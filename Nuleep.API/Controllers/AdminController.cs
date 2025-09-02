using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Nuleep.Business.Interface;
using Nuleep.Business.Services;
using Nuleep.Models;
using Nuleep.Models.Request;

namespace Nuleep.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly IUserService _userService;
        private readonly IConfiguration _config;
        private readonly EmailService _emailService;

        public AdminController(IAdminService adminService, IUserService userService, IConfiguration config, EmailService emailService)
        {
            _adminService = adminService;
            _userService = userService;
            _config = config;
            _emailService = emailService;
        }

        [HttpPost("company/jobs")]
        public async Task<IActionResult> GetJobList([FromBody] JobListRequest request)
        {
            var (jobs, total) = await _adminService.GetJobsByOrg(request.OrgId, request.Limit, request.Page);

            return Ok(new
            {
                success = true,
                data = new
                {
                    data = jobs,
                    total = total
                }
            });
        }

        [HttpPost("company/employees")]
        public async Task<IActionResult> GetEmployeeList([FromBody] EmployeeListRequest request)
        {
            var (employees, total) = await _adminService.GetEmployeeList(request.OrgId, request.Status, request.Limit, request.Page);

            return Ok(new
            {
                success = true,
                data = new
                {
                    data = employees,
                    total = total
                }
            });
        }

        [HttpPut("edit/job")]
        public async Task<IActionResult> EditJob([FromBody] Job request)
        {
            if (request.Id == 0)
                return BadRequest(new { error = "Job ID is required." });

            // Remove commas from salary if provided
            if (!string.IsNullOrEmpty(request.Salary))
                request.Salary = request.Salary.Replace(",", "");

            var updatedJob = await _adminService.EditJob(request);

            if (updatedJob == null)
                return NotFound(new { error = "Job not found" });

            return Ok(new { success = true, data = updatedJob });
        }

        [HttpDelete("delete/job")]
        public async Task<IActionResult> DeleteJob([FromBody] DeleteJobRequest request)
        {
            if (request.Id == 0)
            {
                return StatusCode(500, "Error");
            }

            var deleted = await _adminService.DeleteJob(request.Id);

            if (!deleted)
            {
                return NotFound(new { error = "Job not found" });
            }

            return Ok(new { success = true, data = "Successfully deleted!" });
        }

        [HttpPost("companies")]
        public async Task<IActionResult> GetCompanyList([FromBody] CompanyListRequest request)
        {
            var (list, total) = await _adminService.GetCompanyList(request.Limit, request.Page);

            return Ok(new
            {
                success = true,
                data = new
                {
                    data = list,
                    total = total
                }
            });
        }

        [HttpPost("add/job")]
        public async Task<IActionResult> AddJob([FromBody] JobRequest request)
        {
            if (request.OrgId == 0)
                return StatusCode(500, "Organization ID is required");

            var recruiterId = await _adminService.GetRecruiterIdByOrganization(request.OrgId);

            if (recruiterId == null)
                return NotFound(new { error = "Recruiter not found for this organization" });

            var nuleepId = $"nuleep-{Guid.NewGuid().ToString().Substring(0, 8)}";

            var cleanSalary = request.Salary?.Replace(",", "");

            var job = new Job
            {
                Id = 0,
                PositionTitle = request.PositionTitle,
                Location = request.Location,
                Description = request.Description,
                Department = request.Department,
                Requirements = string.Join(",", request.Requirements),
                SkillKeywords = string.Join(",", request.SkillKeywords),
                JobType = request.JobType,
                SalaryType = request.SalaryType,
                Salary = cleanSalary,
                Remote = request.Remote,
                RequisitionNumber = request.RequisitionNumber,
                PostingDate = request.PostingDate,
                ClosingDate = request.ClosingDate,
                CompanyContact = request.CompanyContact,
                CompanyEmail = request.CompanyEmail,
                NuleepID = nuleepId,
                OrganizationId = request.OrgId,
                RecruiterId = recruiterId.Value
            };

            var newJob = await _adminService.InsertJob(job);    

            return Ok(new { success = true, data = newJob });
        }

        [HttpPost("company")]
        public async Task<IActionResult> GetCompanyById([FromBody] CompanyRequest request)
        {
            var result = await _adminService.GetCompanyById(request.Id);

            if (result == null)
                return Ok(new { success = false, data = "Not Found!" });

            return Ok(new { success = true, data = result });
        }

        [HttpPut("edit/company")]
        public async Task<IActionResult> EditCompanyProfile([FromBody] EditCompanyRequest request)
        {
            var updatedOrg = await _adminService.EditCompanyProfile(request);

            return Ok(new { success = true, data = updatedOrg });
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyRequest request)
        {
            var orgId = await _adminService.CreateCompany(request);
            return Ok(new { success = true, organizationId = orgId });
        }

        [HttpPost("transfer_ownership")]
        public async Task<IActionResult> TransferOwnership([FromBody] TransferOwnershipRequest request)
        {
            try
            {
                var user = await _userService.GetUserByUsername(request.Email);

                if (user == null) {
                    return NotFound(new { error = "No user with that email" });
                }

                var resetToken = GenerateToken(user);
                await _userService.UpdateResetToken(user.Id, resetToken, DateTime.UtcNow.AddDays(1));

                var orgIdBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(request.OrgId));
                var sendObj = new
                {
                    newEmail = request.NewEmail,
                    firstName = request.FirstName,
                    lastName = request.LastName,
                    userId = user.Id
                };

                var sendObjJson = System.Text.Json.JsonSerializer.Serialize(sendObj);
                var sendObjEnc = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(sendObjJson));

                var frontendUrl = _config["FrontendUrl"];
                var resetUrl = $"{frontendUrl}/{orgIdBase64}/forgotpassword/{resetToken}/{sendObjEnc}";

                var subject = "Nuleep Password Reset token";
                var message = $@"
                                Hello,
                                Nuleep admin transferred ownership of your company to you. Please check below link to accept and claim it.
                                Thank you!
                                Please use this link to reset your password: <a href='{resetUrl}' target='_blank'>Click Here!</a>";

                bool ismailSentSuccess = await _emailService.SendEmail(request.NewEmail, subject, message);

                if (ismailSentSuccess) {
                    return Ok(new { success = true, message = "Mail was successfully sent!" });
                }
                else
                {
                    return Ok(new { success = false, message = "Email could not be sent" });
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Email could not be sent", detail = ex.Message });
            }
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
