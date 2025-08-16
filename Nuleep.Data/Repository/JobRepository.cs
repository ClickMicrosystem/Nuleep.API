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
using Nuleep.Models.Request;
using System.Net;
using System.Text;

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


            if (recruiter == null)
            {
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
                                    "SELECT * FROM Profile WHERE UserId = @UserId AND Type = 'recruiter'",
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

        public async Task<(List<Job>, int)> GetAllJobs(JobSearchRequest request)
        {
            var sql = new StringBuilder(@"
                                            SELECT j.Id, j.PositionTitle, j.Location, j.SalaryType, j.Experience, j.JobType, j.Salary, j.ClosingDate, j.PostingDate, o.Name AS OrgName, o.Benefits AS OrgBenefits, o.Culture AS OrgCulture
                                            FROM Jobs j
                                            INNER JOIN Organizations o ON j.OrganizationId = o.Id
                                            WHERE j.ClosingDate >= GETUTCDATE()
                                        ");

            var parameters = new DynamicParameters();

            // 🔎 Company Name
            if (!string.IsNullOrEmpty(request.CompanyName))
            {
                sql.Append(" AND o.Name LIKE @CompanyName");
                parameters.Add("@CompanyName", $"%{request.CompanyName}%");
            }

            // 🔎 Benefits
            if (request.Benefits != null && request.Benefits.Any())
            {
                sql.Append(" AND (");
                for (int i = 0; i < request.Benefits.Count; i++)
                {
                    if (i > 0) sql.Append(" OR ");
                    sql.Append($"o.Benefits LIKE @Benefit{i}");
                    parameters.Add($"@Benefit{i}", $"%{request.Benefits[i]}%");
                }
                sql.Append(")");
            }

            // 🔎 Culture
            if (!string.IsNullOrEmpty(request.Culture))
            {
                sql.Append(" AND o.Culture LIKE @Culture");
                parameters.Add("@Culture", $"%{request.Culture}%");
            }

            // 🔎 Job title
            if (!string.IsNullOrEmpty(request.Name))
            {
                sql.Append(" AND j.PositionTitle LIKE @PositionTitle");
                parameters.Add("@PositionTitle", $"%{request.Name}%");
            }

            // 🔎 Location
            if (!string.IsNullOrEmpty(request.Location))
            {
                sql.Append(" AND j.Location LIKE @Location");
                parameters.Add("@Location", $"%{request.Location}%");
            }

            // 🔎 Compensation
            if (request.Compensation != null && request.Compensation.Any())
            {
                sql.Append(" AND j.SalaryType IN @Compensation");
                parameters.Add("@Compensation", request.Compensation);
            }

            // 🔎 Experience
            if (request.Experience != null && request.Experience.Any())
            {
                sql.Append(" AND j.Experience IN @Experience");
                parameters.Add("@Experience", request.Experience);
            }

            // 🔎 JobType
            if (request.JobType != null && request.JobType.Any())
            {
                sql.Append(" AND j.JobType IN @JobType");
                parameters.Add("@JobType", request.JobType);
            }

            // 🔎 Salary Range
            if (request.MaxSalary.HasValue && request.MaxSalary.Value > 0)
            {
                sql.Append(" AND j.Salary BETWEEN @MinSalary AND @MaxSalary");
                parameters.Add("@MinSalary", request.MinSalary ?? 0);
                parameters.Add("@MaxSalary", request.MaxSalary ?? int.MaxValue);
            }

            // 🔎 Skills
            if (request.Skills != null && request.Skills.Any())
            {
                sql.Append(" AND EXISTS (SELECT 1 FROM JobSkills js WHERE js.JobId = j.Id AND js.Skill IN @Skills)");
                parameters.Add("@Skills", request.Skills);
            }

            // Sorting
            sql.Append(request.PostingDateSort == -1
                ? " ORDER BY j.PostingDate DESC"
                : " ORDER BY j.PostingDate ASC");

            // Pagination
            sql.Append(" OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY");
            parameters.Add("@Offset", (request.Page - 1) * request.Limit);
            parameters.Add("@Limit", request.Limit);

            // Query data
            List<Job> jobs = (await _db.QueryAsync<Job>(sql.ToString(), parameters)).ToList();

            // Query total count (without paging)
            var countSql = $"SELECT COUNT(*) FROM Jobs j INNER JOIN Organizations o ON j.OrganizationId = o.Id WHERE j.ClosingDate >= GETUTCDATE()";
            var total = await _db.ExecuteScalarAsync<int>(countSql, parameters);

            return (jobs, total);
        }

        public async Task<IEnumerable<int>> GetJobIdsByRecruiter(int recId)
        {
            var sql = "SELECT Id FROM Jobs WHERE RecruiterId = @RecId";
            return await _db.QueryAsync<int>(sql, new { RecId = recId });
        }

        public async Task UpdateJobsRecruiter(IEnumerable<int> jobIds, int newRecId)
        {
            var sql = "UPDATE Jobs SET RecruiterId = @NewRecId WHERE Id IN @Ids";
            await _db.ExecuteAsync(sql, new { NewRecId = newRecId, Ids = jobIds });
        }

        public async Task MarkUserDeleted(int userId)
        {
            var sql = "UPDATE Users SET IsDelete = 1 WHERE Id = @UserId";
            await _db.ExecuteAsync(sql, new { UserId = userId });
        }

        public async Task MarkProfileDeleted(int profileId)
        {
            var sql = "UPDATE Profile SET IsDelete = 1 WHERE Id = @ProfileId";
            await _db.ExecuteAsync(sql, new { ProfileId = profileId });
        }
    }
}
