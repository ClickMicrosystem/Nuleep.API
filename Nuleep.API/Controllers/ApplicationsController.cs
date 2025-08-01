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
            
            var recruterAppData = await _applicationService.GetAllRecruiterApplications(userId.ToString());

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

            var recruterAppData = await _applicationService.GetAllJobSeekerApplications(userId.ToString());

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
            var userId = int.Parse(User.Claims.ToList()[0].Value);

            var applicationData = await _applicationService.GetApplicationsByJob(jobId, userId);

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
            var userId = int.Parse(User.Claims.ToList()[0].Value);

            var newApplication = await _applicationService.CreateApplication(jobId, application, userId);

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

            var result = await _applicationService.GetApplicationById(applicationId, UserId);
            if (result == null)
                return NotFound(new { error = "Profile not found or unauthorized" });

            return Ok(new { success = true, data = result });
        }

        [HttpPut("{applicationId}")]
        public async Task<IActionResult> EditApplication(int applicationId, [FromBody] Application request)
        {
            var userId = int.Parse(User.Claims.ToList()[0].Value);

            var result = await _applicationService.UpdateApplication(applicationId, userId, request);
            if (result == null)
                return BadRequest(new { error = "Not Authorized or Application not found" });

            return Ok(new { success = true, data = result });
        }

        [HttpDelete("{applicationId}")]
        public async Task<IActionResult> DeleteApplication(int applicationId)
        {
            var userId = int.Parse(User.Claims.ToList()[0].Value);

            var success = await _applicationService.DeleteApplication(applicationId, userId);
            if (!success)
                return BadRequest(new { error = "Not authorized or Application not found" });

            return Ok(new { success = true, data = new object[] { } });
        }


    }
}
