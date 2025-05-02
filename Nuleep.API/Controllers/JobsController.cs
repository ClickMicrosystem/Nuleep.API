using System.Data;
using System.Security.Claims;
using Azure.Core;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Nuleep.Business.Interface;
using Nuleep.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Nuleep.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly IJobService _jobService;

        public JobsController(IJobService jobService)
        {
            _jobService = jobService;
        }

        [HttpGet]
        public async Task<IActionResult> CreateJob(Job job)
        {
            var userId = int.Parse(User.FindFirst("id")?.Value ?? "0");

            var user = await _jobService.CreateJob(userId, job);
            if (user == null)
                return NotFound();

            return Ok(new { success = true, data = user });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetJob(int id)
        {

            var job = await _jobService.GetJobById(id);
            

            if (job == null)
            {
                return NotFound(new { error = "Job not found" });
            }

            return Ok(new { success = true, data = job });
        }

        [HttpGet("recruiter/all")]
        public async Task<IActionResult> GetAllRecruiterJobs()
        {

            var allRecruiterJobs = await _jobService.GetAllRecruiterJobs();

            return Ok(new
            {
                success = true,
                data = allRecruiterJobs.Values
            });
        }


    }
}
