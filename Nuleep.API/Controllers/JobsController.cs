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
        private readonly IDbConnection _db;

        public JobsController(IJobService jobService, IConfiguration config)
        {
            _jobService = jobService;
            _db = new SqlConnection(config.GetConnectionString("DefaultConnection"));
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateJob(Job job)
        {
            var userId = int.Parse(User.Claims.ToList()[0].Value);
            var createdJob = await _jobService.CreateJob(userId, job);
            if (createdJob == null || createdJob.code == 1)
                return NotFound();

            return Ok(new { success = true, data = createdJob.data });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateJob(int userId, int jobId, Job updatedJob)
        {
            // Get recruiter profile by user ID
            var recruiter = await _db.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT * FROM Profiles WHERE UserId = @UserId AND Type = 'recruiter'",
                new { UserId = userId });

            // Get job by ID
            var existingJob = await _db.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT * FROM Jobs WHERE Id = @JobId",
                new { JobId = jobId });

            if (existingJob == null || recruiter == null)
            {
                return new NotFoundObjectResult(new { error = "Resource not found" });
            }

            // Ensure recruiter is authorized to edit this job
            if ((int)existingJob.RecruiterProfileId != (int)recruiter.Id)
            {
                return new BadRequestObjectResult(new { error = "You are not authorized to edit this organization" });
            }


            var updateQuery = @"
        UPDATE Jobs
        SET 
            Title = @Title,
            Description = @Description,
            Salary = @Salary,
            Location = @Location,
            Department = @Department,
            JobType = @JobType,
            SalaryType = @SalaryType,
            Remote = @Remote,
            RequisitionNumber = @RequisitionNumber,
            PostingDate = @PostingDate,
            ClosingDate = @ClosingDate,
            CompanyContact = @CompanyContact,
            CompanyEmail = @CompanyEmail,
            Program = @Program,
            Experience = @Experience,
            ExperienceLevel = @ExperienceLevel
        WHERE Id = @JobId";

            await _db.ExecuteAsync(updateQuery, new
            {
                updatedJob.PositionTitle,
                updatedJob.Description,
                Salary = updatedJob.Salary,
                updatedJob.Location,
                updatedJob.Department,
                updatedJob.JobType,
                updatedJob.SalaryType,
                updatedJob.Remote,
                updatedJob.RequisitionNumber,
                updatedJob.PostingDate,
                updatedJob.ClosingDate,
                updatedJob.CompanyContact,
                updatedJob.CompanyEmail,
                updatedJob.Program,
                updatedJob.Experience,
                updatedJob.ExperienceLevel,
                JobId = jobId
            });

            return new OkObjectResult(new
            {
                success = true,
                data = new
                {
                    jobId,
                    updatedJob.PositionTitle,
                    updatedJob.Description,
                    Salary = updatedJob.Salary      
                }
            });
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
            var userId = int.Parse(User.Claims.ToList()[0].Value);

            ResponeModel allRecruiterJobs = await _jobService.GetAllRecruiterJobs(userId);

            if(allRecruiterJobs.code == 1)
            {
                return NotFound(new { error = "Recruiter profile not found" });
            }

            return Ok(new
            {
                success = true,
                data = allRecruiterJobs.data
            });
        }

        // @desc      Delete a job
        // @route     DELETE /api/jobs/:jobID
        // @access    Private
        [HttpDelete("{jobId}")]
        [Authorize]
        public async Task<IActionResult> DeleteJob(int jobId)
        {
            var userId = int.Parse(User.Claims.ToList()[0].Value).ToString();

            var recruiterSql = "SELECT * FROM Recruiters WHERE UserRef = @UserId";
            var jobSql = "SELECT * FROM Jobs WHERE Id = @JobId";
            var deleteJobSql = "DELETE FROM Jobs WHERE Id = @JobId";
            var deleteApplicationsSql = "DELETE FROM Applications WHERE JobId = @JobId";


            var recruiter = await _db.QueryFirstOrDefaultAsync<Recruiter>(recruiterSql, new { UserId = userId });
            var job = await _db.QueryFirstOrDefaultAsync<Job>(jobSql, new { JobId = jobId });

            if (recruiter == null || job == null)
                return NotFound(new { error = "Resource not found" });

            if (job.RecruiterId != recruiter.Id)
                return BadRequest(new { error = "You are not authorized to edit this organization" });

            await _db.ExecuteAsync(deleteJobSql, new { JobId = jobId });
            await _db.ExecuteAsync(deleteApplicationsSql, new { JobId = jobId });

            return Ok(new { success = true });
        }



    }
}
