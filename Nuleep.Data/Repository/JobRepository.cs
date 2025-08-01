using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Nuleep.Models;
using System.Data;
using Dapper;
using Nuleep.Models.Blogs;
using Azure.Core;
using Nuleep.Data.Interface;
using static System.Runtime.InteropServices.JavaScript.JSType;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Bibliography;
using Nuleep.Models.Response;

namespace Nuleep.Data.Repository
{
    public class JobRepository : IJobRepository
    {
        private readonly IDbConnection _db;

        public JobRepository(IConfiguration config)
        {
            _db = new SqlConnection(config.GetConnectionString("DefaultConnection"));
        }

        public async Task<ResponeModel> CreateJob(int userId, Job job)
        {
            ResponeModel responeModel = new ResponeModel();

            int profileId = await _db.QueryFirstOrDefaultAsync<int>(
                                    "SELECT Id FROM Profile WHERE UserRef = @UserId AND Type = 'recruiter'",
                                    new { UserId = userId });

            if (profileId == 0)
            {
                responeModel.code = 1;
                responeModel.data = null;
                return responeModel;
            }

            var recruiter = await _db.QueryFirstOrDefaultAsync<Recruiter>(
                                    "Select * from Recruiters Where ProfileId = @ProfileId",
                                    new { ProfileId = profileId });


            if (recruiter == null) {
                responeModel.code = 1;
                responeModel.data = recruiter;
                return responeModel;
            }

            int orgId = await _db.QueryFirstOrDefaultAsync<int>(
                        "SELECT Id FROM Organizations WHERE ProfileId = @ProfileId",
                        new { ProfileId = profileId });

           var nuleepId = $"nuleep-{Guid.NewGuid().ToString("N").Substring(0, 8)}";

            job.Salary = job.Salary.Replace(",", "");

            var jobInsertQuery = @"
                                    INSERT INTO Jobs (PositionTitle, Experience, Location, Description, Department, Requirements, SkillKeywords, JobType, SalaryType, Salary, Remote,
                                                      RequisitionNumber, PostingDate, ClosingDate, CompanyContact, CompanyEmail, OrganizationId,
                                                      RecruiterId, Program, ExperienceLevel, NuleepID)
                                    VALUES (@PositionTitle, @Experience, @Location, @Description, @Department, @Requirements, @SkillKeywords, @JobType, @SalaryType, @Salary, @Remote,
                                            @RequisitionNumber, @PostingDate, @ClosingDate, @CompanyContact, @CompanyEmail, @OrganizationId, @RecruiterId, @Program, @ExperienceLevel, @NuleepID);
                                    SELECT CAST(SCOPE_IDENTITY() as int);";

            var jobId = await _db.ExecuteScalarAsync<int>(jobInsertQuery, new
            {
                PositionTitle = job.PositionTitle,
                Experience = job.Experience,
                Location = job.Location,
                Description = job.Description,
                Department = job.Department,
                Requirements = string.Join(",", job.Requirements),
                SkillKeywords = string.Join(",", job.SkillKeywords),
                JobType = job.JobType,
                SalaryType = job.SalaryType,
                Salary = job.Salary,
                Remote = job.Remote,
                RequisitionNumber = job.RequisitionNumber,
                PostingDate = job.PostingDate,
                ClosingDate = job.ClosingDate,
                CompanyContact = job.CompanyContact,
                CompanyEmail = job.CompanyEmail,
                OrganizationId = orgId,
                RecruiterId = recruiter.Id,
                Program = job.Program,
                ExperienceLevel = job.ExperienceLevel,
                NuleepID = nuleepId
            });            

            responeModel.data = new
            {
                _id = jobId,
                PositionTitle = job.PositionTitle,
                Experience = job.Experience,
                Location = job.Location,
                Description = job.Description,
                Requirements = string.Join(",", job.Requirements),
                SkillKeywords = string.Join(",", job.SkillKeywords),
                JobType = job.JobType,
                SalaryType = job.SalaryType,
                Salary = job.Salary,
                Remote = job.Remote,
                RequisitionNumber = job.RequisitionNumber,
                PostingDate = job.PostingDate,
                ClosingDate = job.ClosingDate,
                CompanyContact = job.CompanyContact,
                CompanyEmail = job.CompanyEmail,
                OrganizationId = orgId,
                RecruiterId = recruiter.Id,
                Program = job.Program,
                ExperienceLevel = job.ExperienceLevel,
                NuleepID = nuleepId
            };

            return responeModel;
        }

        public async Task<dynamic> UpdateJob(int userId, Job job)
        {

            var recruiter = await _db.QueryFirstOrDefaultAsync<dynamic>(
                                    "SELECT * FROM Profiles WHERE UserId = @UserId AND Type = 'recruiter'",
                                    new { UserId = userId });

            if (recruiter != null)
            {
                return new { data = recruiter, code = 1 };
            }

            var nuleepId = $"nuleep-{Guid.NewGuid().ToString("N").Substring(0, 8)}";

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

            return new
            {
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

            var result = await _db.QueryAsync<JobsResponse, Organization, Recruiter, JobsResponse>(
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

        public async Task<ResponeModel> GetAllRecruiterJobs(int userId)
        {
            ResponeModel responeModel = new ResponeModel();

            // 1. Find recruiter profile
            var recruiterId = await _db.QueryFirstOrDefaultAsync<int>(
                "Select * from Recruiters where ProfileId = (SELECT Id FROM Profile WHERE UserRef = @UserId AND Type = 'recruiter')",
                new { UserId = userId }
            );

            if (recruiterId == 0)
            {
                responeModel.code = 1;
                return responeModel;
            }

            var sql = @"
                        SELECT 
                            j.Id, j.ClosingDate,
                            o.Id, o.Name,
                            a.Id, a.JobId, a.ProfileId,
                            p.Id, p.FullName
                        FROM Jobs j
                        INNER JOIN Organizations o ON j.OrganizationId = o.Id
                        LEFT JOIN Applications a ON a.JobId = j.Id
                        LEFT JOIN Profile p ON a.ProfileId = p.Id
                        WHERE j.RecruiterId = @RecruiterId AND j.ClosingDate >= GETUTCDATE()
                        ORDER BY j.ClosingDate DESC";

            var jobDictionary = new Dictionary<int, JobsResponse>();

            var jobs = await _db.QueryAsync<JobsResponse, Organization, ApplicationResponse, Profile, JobsResponse>(
                sql,
                (job, org, app, profile) =>
                {
                    if (!jobDictionary.TryGetValue(job.Id, out var jobEntry))
                    {
                        job.Organization = org;
                        job.Application = new List<ApplicationResponse>();
                        jobDictionary[job.Id] = job;
                        jobEntry = job;
                    }

                    if (app != null && profile != null)
                    {
                        app.Profile = profile;
                        jobEntry.Application.Add(app);
                    }

                    return jobEntry;
                },
                new { RecruiterId = recruiterId },
                splitOn: "Id,Id,Id,Id" // adjust split keys if needed
            );


            responeModel.data = jobDictionary.Values;
            return responeModel;
            
        }
    
    
    }
}
