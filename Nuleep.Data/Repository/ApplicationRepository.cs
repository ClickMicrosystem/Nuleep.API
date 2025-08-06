using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Nuleep.Models;
using System.Data;
using Dapper;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Nuleep.Data.Interface;
using Nuleep.Models.Response;

namespace Nuleep.Data.Repository
{
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly IDbConnection _db;

        public ApplicationRepository(IConfiguration config)
        {
            _db = new SqlConnection(config.GetConnectionString("DefaultConnection"));
        }

        public async Task<dynamic> GetAllRecruiterApplications(string userId)
        {
            ResponeModel responeModel = new ResponeModel();
            var profileId = await _db.QueryFirstOrDefaultAsync<int>(
                            "SELECT Id FROM Profile WHERE UserRef = @UserId",
                            new { UserId = userId });
            
            var recruiter = await _db.QueryFirstOrDefaultAsync<Recruiter>(
                            "SELECT * FROM Recruiters WHERE ProfileId = @ProfileId",
                            new { ProfileId = profileId });

            if(recruiter == null)
            {
                responeModel.code = 1;
                return responeModel;
            }


            var jobs = (await _db.QueryAsync<dynamic>(
            @"SELECT * FROM Jobs 
              WHERE RecruiterId = @RecruiterId 
              AND ClosingDate >= GETUTCDATE()",
            new { RecruiterId = recruiter.Id })).ToList();

            if (jobs.Count() == 0)
            {
                responeModel.code = 2;
                return responeModel;
            }

            var jobIds = jobs.Select(j => (int)j.Id).ToArray();

            var applications = await _db.QueryAsync<ApplicationResponse, JobsResponse, Profile, ApplicationResponse>(
                                @"SELECT a.*, j.*, p.*
                                FROM Applications a
                                INNER JOIN Jobs j ON a.JobId = j.Id
                                INNER JOIN Profile p ON a.ProfileId = p.Id
                                WHERE a.JobId IN @JobIds",
                                (a, j, p) =>
                                {
                                    a.Job = j;
                                    a.Profile = p;
                                    return a;
                                },
                                new { JobIds = jobIds },
                                splitOn: "Id,Id");

            return responeModel;

        }
        public async Task<dynamic> GetAllJobSeekerApplications(string userId)
        {
            ResponeModel responeModel = new ResponeModel();
            var profileId = await _db.QueryFirstOrDefaultAsync<int>(
                            "SELECT Id FROM Profile WHERE UserRef = @UserId",
                            new { UserId = userId });
            
            var jobSeeker = await _db.QueryFirstOrDefaultAsync<JobSeeker>(
                            "SELECT * FROM JobSeekers WHERE ProfileId = @ProfileId",
                            new { ProfileId = profileId });

            if(jobSeeker == null)
            {
                responeModel.code = 2;
                return responeModel;
            }

            var sql = @"
                        SELECT 
                            a.Id, a.ProfileId, a.CreatedAt,
                            j.Id,
                            o.Id
                        FROM Applications a
                        INNER JOIN Jobs j ON a.JobId = j.Id
                        INNER JOIN Organizations o ON j.OrganizationId = o.Id
                        WHERE a.ProfileId = @ProfileId
                        ORDER BY a.CreatedAt DESC";

            var appDictionary = new Dictionary<int, ApplicationResponse>();

            try
            {
                var result = await _db.QueryAsync<ApplicationResponse, JobsResponse, Organization, ApplicationResponse>(
                sql,
                (app, job, org) =>
                {
                    if (!appDictionary.TryGetValue(app.Id, out var existingApp))
                    {
                        app.Job = job;
                        job.Organization = org;
                        appDictionary.Add(app.Id, app);
                    }
                    return app;
                },
                new { ProfileId = profileId }
            );
            }
            catch(Exception e)
            {

            }

            
            return responeModel;
        }

        public async Task<dynamic> GetApplicationsByJob(int jobId, int userId)
        {

            // Step 1: Get recruiter profile by logged-in userId
            // Step 1: Get recruiter profile by logged-in userId
            ResponeModel responeModel = new ResponeModel();
            var recruiter = await _db.QueryFirstOrDefaultAsync<dynamic>(
                                "SELECT Id FROM Profile WHERE UserId = @UserId AND Type = 'recruiter'",
                                new { UserId = userId }
                            );

            if (recruiter == null)
            {
                responeModel.code = 1;
                responeModel.data = recruiter;
                return responeModel;
            }

            // Step 2: Get job
            var job = await _db.QueryFirstOrDefaultAsync<dynamic>(
                            "SELECT Id, RecruiterId FROM Jobs WHERE Id = @JobId",
                            new { JobId = jobId }
                        );

            if (job == null)
            {
                responeModel.code = 2;
                responeModel.data = job;
                return responeModel;
            }

            // Step 3: Check if recruiter owns the job
            if (job.RecruiterId != recruiter.Id)
            {
                responeModel.code = 3;
                responeModel.data = job;
                return responeModel;
            }

            // Step 4: Get applications for the job with populated Job and Profile
            var sql = @"
                        SELECT 
                            a.*, 
                            j.Id, j.PositionTitle, j.Location, 
                            p.Id, p.FullName, p.Email
                        FROM Applications a
                        LEFT JOIN Jobs j ON a.JobId = j.Id
                        LEFT JOIN Profile p ON a.ProfileId = p.Id
                        WHERE a.JobId = @JobId
                    ";

            var applications = await _db.QueryAsync<ApplicationResponse, JobsResponse, Profile, ApplicationResponse>(
                                    sql,
                                    (application, job, profile) =>
                                    {
                                        application.Job = job;
                                        application.Profile = profile;
                                        return application;
                                    },
                                    new { JobId = jobId },
                                    splitOn: "Id,Id"
                                );

            responeModel.data = applications;
            return responeModel;
        }

        public async Task<dynamic> CreateApplication(int jobId, Application application, int userId)
        {
            var job = await _db.QueryFirstOrDefaultAsync<Job>("SELECT * FROM Jobs WHERE Id = @JobId", new { JobId = jobId });

            if (job == null)
            {
                return new { data = job, code = 1 };
            }

            // Get job seeker profile for user
            var profile = await _db.QueryFirstOrDefaultAsync<JobSeeker>(
                "SELECT * FROM JobSeekers WHERE UserRef = @UserId", new { UserId = userId });

            if (profile == null)
            {
                return new { data = profile, code = 2 };
            }

            var sqlInsert = @"
                            INSERT INTO Applications (Status, JobId, ProfileId, CoverLetter, IsRemoved, IsArchived, IsSaved)
                            OUTPUT INSERTED.*
                            VALUES (@Status, @JobId, @ProfileId, @CoverLetter, @IsRemoved, @IsArchived, @IsSaved)";

            var parameters = new
            {
                application.Status,
                JobId = jobId,
                ProfileId = profile.Id,
                application.CoverLetter,
                application.IsRemoved,
                application.IsArchived,
                application.IsSaved
            };

            var newApplication = await _db.QuerySingleAsync<Application>(sqlInsert, parameters);

            return new { data = newApplication, code = 0 };
        }

        public async Task<ApplicationDetail?> GetApplicationById(int applicationId)
        {
            using var multi = await _db.QueryMultipleAsync(@"
                                SELECT * FROM Applications WHERE Id = @Id;
                                SELECT p.* FROM Profile p
                                    INNER JOIN Applications a ON a.ProfileId = p.Id
                                    WHERE a.Id = @Id;
                                SELECT j.* FROM Jobs j
                                    INNER JOIN Applications a ON a.JobId = j.Id
                                    WHERE a.Id = @Id;",
                                        new { Id = applicationId });

            var application = await multi.ReadFirstOrDefaultAsync<ApplicationDetail>();
            if (application == null) return null;

            application.Profile = await multi.ReadFirstOrDefaultAsync<Profile>();
            application.Job = await multi.ReadFirstOrDefaultAsync<Job>();

            return application;
        }

        public async Task<ApplicationDetail?> UpdateApplication(int applicationId, Application request)
        {
            var sql = @"
                        UPDATE Applications
                        SET Status = @Status,
                            CoverLetter = @CoverLetter,
                            IsRemoved = @IsRemoved,
                            IsArchived = @IsArchived,
                            IsSaved = @IsSaved,
                        WHERE Id = @Id;
                        SELECT CAST(SCOPE_IDENTITY() as int);";

            await _db.QueryFirstOrDefaultAsync<dynamic>(sql, new
            {
                Id = applicationId,
                Status = request.Status,
                CoverLetter = request.CoverLetter,
                IsRemoved = request.IsRemoved,
                IsArchived = request.IsArchived,
                IsSaved = request.IsSaved
            });
            return await GetApplicationById(applicationId);
        }

        public async Task<bool> DeleteApplication(int applicationId)
        {
            var sql = "DELETE FROM Applications WHERE Id = @Id";
            var rows = await _db.ExecuteAsync(sql, new { Id = applicationId });
            return rows > 0;
        }

    }
}
