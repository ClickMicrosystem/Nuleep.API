using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Nuleep.Models;
using System.Data;
using Dapper;
using Nuleep.Models.Blogs;
using Azure.Core;
using Nuleep.Data.Interface;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Nuleep.Data.Repository
{
    public class JobRepository : IJobRepository
    {
        private readonly IDbConnection _db;

        public JobRepository(IConfiguration config)
        {
            _db = new SqlConnection(config.GetConnectionString("DefaultConnection"));
        }

        public async Task<dynamic> CreateJob(int userId, Job job)
        {

            var recruiter = await _db.QueryFirstOrDefaultAsync<dynamic>(
                                    "SELECT * FROM Profiles WHERE UserId = @UserId AND Type = 'recruiter'",
                                    new { UserId = userId });

            if (recruiter != null) {
                return new { data = recruiter, code = 1};                
            }

            // Step 2: Generate unique nuleepID
            var nuleepId = $"nuleep-{Guid.NewGuid().ToString("N").Substring(0, 8)}";

            // Step 3: Clean Salary (remove commas)
            //var cleanedSalary = job.Salary.Replace(",", "");

            // Step 4: Insert Job


            var jobInsertQuery = @"
                                    INSERT INTO Jobs (PositionTitle, Experience, Location, Description, Department, JobType, SalaryType, Salary, Remote,
                                                      RequisitionNumber, PostingDate, ClosingDate, CompanyContact, CompanyEmail, OrganizationId,
                                                      RecruiterId, Program, ExperienceLevel, NuleepID)
                                    VALUES (@PositionTitle, @Experience, @Location, @Description, @Department, @JobType, @SalaryType, @Salary, @Remote,
                                            @RequisitionNumber, @PostingDate, @ClosingDate, @CompanyContact, @CompanyEmail, @OrganizationId,
                                            @RecruiterId, @Program, @ExperienceLevel, @NuleepID);
                                    SELECT CAST(SCOPE_IDENTITY() as int);";

            var insertJobQuery = @"
                                    INSERT INTO Jobs (NuleepID, Title, Description, Salary, OrganizationId, RecruiterProfileId, CreatedAt)
                                    VALUES (@NuleepID, @Title, @Description, @Salary, @OrganizationId, @RecruiterProfileId, GETDATE());
                                    SELECT CAST(SCOPE_IDENTITY() as int);
                                ";

            var jobId = await _db.ExecuteScalarAsync<int>(insertJobQuery, new
            {
                NuleepID = nuleepId,
                job.PositionTitle,
                job.Description,
                Salary = job.Salary,
                OrganizationId = recruiter.OrganizationId,
                RecruiterProfileId = recruiter.Id
            });            

            return  new { 
                data = new
                {
                    Id = jobId,
                    NuleepID = nuleepId,
                    job.PositionTitle,
                    job.Description,
                    Salary = job.Salary,
                    OrganizationId = recruiter.OrganizationId,
                    RecruiterProfileId = (int)recruiter.Id
                }
            };            
        }
    

        public async Task<dynamic> GetJobById(int id)
        {


            var sql = @"
                            SELECT 
                                j.*, 
                                o.Id, o.Name, o.Email, 
                                r.Id, r.FirstName, r.LastName
                            FROM Jobs j
                            LEFT JOIN Organizations o ON j.OrganizationId = o.Id
                            LEFT JOIN Recruiters r ON j.RecruiterId = r.Id
                            WHERE j.Id = @Id;
                        ";

            var result = await _db.QueryAsync<Job, Organization, Recruiter, Job>(
                sql,
                (job, organization, recruiter) =>
                {
                    job.Organization = organization;
                    job.Recruiter = recruiter;
                    return job;
                },
                new { Id = id },
                splitOn: "Id,Id"
            );

            return result.FirstOrDefault();
        }

        private int GetLoggedInUserId()
        {
            return 2;
            //return int.Parse(User.FindFirst("id").Value); // assuming JWT has user id in claim "id"
        }

        public async Task<dynamic> GetAllRecruiterJobs()
        {
            var userId = GetLoggedInUserId();

            // 1. Find recruiter profile
            var recruiter = await _db.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT * FROM Profiles WHERE UserId = @UserId AND Type = 'recruiter'",
                new { UserId = userId }
            );

            if (recruiter == null)
            {
                return (new { data = recruiter, code = 1 });
                //return NotFound(new { error = "Recruiter profile not found" });

            }

            // 2. Get recruiter's jobs (with Organization and Applications and Application's Profile)
            var sql = @"
                        SELECT 
                            j.*, 
                            o.Id as OrganizationId, o.Name as OrganizationName,
                            a.Id as ApplicationId, a.ProfileId,
                            p.FirstName, p.LastName
                        FROM Jobs j
                        LEFT JOIN Organizations o ON j.OrganizationId = o.Id
                        LEFT JOIN Applications a ON a.JobId = j.Id
                        LEFT JOIN Profiles p ON a.ProfileId = p.Id
                        WHERE j.RecruiterId = @RecruiterId
                          AND (j.ClosingDate IS NULL OR j.ClosingDate >= GETUTCDATE())
                        ORDER BY j.PostingDate DESC
                    ";

            var jobDictionary = new Dictionary<int, Recruiter>();

            //var jobs = await _db.QueryAsync<Recruiter, Application, Recruiter>(
            //    sql,
            //    (job, application) =>
            //    {

            //        if (!jobDictionary.TryGetValue(job.Id, out var jobEntry))
            //        {
            //            jobEntry = job;
            //            jobEntry.Applications = new List<ApplicationResponse>();
            //            jobDictionary.Add(job.Id, jobEntry);
            //        }

            //        if (application != null && application.ApplicationId != 0)
            //        {
            //            jobEntry.Applications.Add(application);
            //        }

            //        return jobEntry;
            //    },
            //    new { RecruiterId = recruiter.Id },
            //    splitOn: "ApplicationId"
            //);

            return (new { data = recruiter, code = 0 });
            
        }
    
    
    }
}
