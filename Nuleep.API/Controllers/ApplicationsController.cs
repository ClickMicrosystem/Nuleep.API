using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Nuleep.Business.Interface;
using Nuleep.Models;

namespace Nuleep.API.Controllers
{
    [ApiController]
    [Route("api/applications")]
    public class ApplicationsController : ControllerBase
    {
        private readonly IApplicationService _applicationService;

        public ApplicationsController(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        [HttpGet("recruiter/all")]
        [Authorize]
        public async Task<IActionResult> GetAllRecruiterApplications()
        {
            int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            GenericClassResponse<dynamic> recruterAppData = await _applicationService.GetAllRecruiterApplications(userId.ToString());

            if (recruterAppData != null) {
                if (recruterAppData.Code == 1) {
                    return NotFound(new { success = false, error = "Recruiter profile not found" });
                }
                else if (recruterAppData.Code == 2)
                {
                    return NotFound(new { success = false, error = "Jobs for this profile not found." });
                }                
            }

            return Ok(new { success = true, data = recruterAppData?.Data ?? Array.Empty<object>() });
        }

        [HttpGet("jobSeeker/all")]
        [Authorize]
        public async Task<IActionResult> GetAllJobSeekerApplications()
        {
            var userId = int.Parse(User.Claims.ToList()[0].Value).ToString();

            GenericClassResponse<dynamic> recruterAppData = await _applicationService.GetAllJobSeekerApplications(userId.ToString());

            if (recruterAppData != null) {
                if (recruterAppData.Code == 1) {
                    return NotFound(new { success = false, error = "Recruiter profile not found" });
                }
                else if (recruterAppData.Code == 2)
                {
                    return NotFound(new { success = false, error = "Jobs for this profile not found." });
                }                
            }

            return Ok(new { success = true, data = recruterAppData?.Data ?? Array.Empty<object>() });
        }

        [HttpGet("job/{jobId}")]
        public async Task<IActionResult> GetApplicationsByJob(int jobId)
        {

            var applicationData = await _applicationService.GetApplicationsByJob(jobId);

            if(applicationData.code == 1)
            {
                return NotFound(new { error = "Recruiter profile not found" });
            }
            else if (applicationData.code == 2)
            {
                return NotFound(new { error = "Job not found" });
            }
            else if (applicationData.code == 3)
            {
                return BadRequest(new { error = "You do not have permission to view these job applications" });
            }

            return Ok(new { success = true, data = applicationData });
        }

        [HttpPost("job/{jobId}")]
        [Authorize]
        public async Task<IActionResult> CreateApplication(int jobId, [FromBody] Application application)
        {
            var UserId = int.Parse(User.Claims.ToList()[0].Value);

            var newApplication = await _applicationService.CreateApplication(jobId, application);

            if (newApplication.code == 1)
            {
                return NotFound(new { error = "Job not found" });
            }
            else if (newApplication.code == 2)
            {
                return NotFound(new { error = "Job Seeker Profile not found" });
            }            

            return Ok(new { success = true, data = newApplication });
        }

        [HttpGet("{applicationId}")]
        public async Task<IActionResult> GetApplication(int applicationId)
        {
            var UserId = int.Parse(User.Claims.ToList()[0].Value);

            var result = await _applicationService.GetApplicationByIdAsync(applicationId, userId);
            if (result == null)
                return NotFound(new { error = "Profile not found or unauthorized" });

            return Ok(new { success = true, data = result });
        }

    }
}
