﻿using Microsoft.Data.SqlClient;
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
            var profileId = await _db.QueryFirstOrDefaultAsync<int>(
                            "SELECT Id FROM Profile WHERE UserRef = @UserId",
                            new { UserId = userId });
            
            var recruiter = await _db.QueryFirstOrDefaultAsync<Recruiter>(
                            "SELECT * FROM Recruiters WHERE ProfileId = @ProfileId",
                            new { ProfileId = profileId });

            if(recruiter == null)
            {
                return GenericClassResponse<dynamic>.Create(recruiter, 1);
            }


            var jobs = (await _db.QueryAsync<dynamic>(
            @"SELECT * FROM Jobs 
              WHERE RecruiterId = @RecruiterId 
              AND ClosingDate >= GETUTCDATE()",
            new { RecruiterId = recruiter.Id })).ToList();

            if (jobs.Count() == 0)
            {
                return GenericClassResponse<dynamic>.Create(jobs, 2);
            }

            var jobIds = jobs.Select(j => (int)j.Id).ToArray();

            var applications = await _db.QueryAsync<Application, Job, Profile, Application>(
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

            //turn new { data = applications, code = 0 };
            return GenericClassResponse<dynamic>.Create(applications, 0);

        }
        public async Task<dynamic> GetAllJobSeekerApplications(string userId)
        {
            var profileId = await _db.QueryFirstOrDefaultAsync<int>(
                            "SELECT Id FROM Profile WHERE UserRef = @UserId",
                            new { UserId = userId });
            
            var jobSeeker = await _db.QueryFirstOrDefaultAsync<JobSeeker>(
                            "SELECT * FROM JobSeekers WHERE ProfileId = @ProfileId",
                            new { ProfileId = profileId });

            if(jobSeeker == null)
            {
                return GenericClassResponse<dynamic>.Create(jobSeeker, 1);
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

            var appDictionary = new Dictionary<int, Application>();

            try
            {
                var result = await _db.QueryAsync<Application, Job, Organization, Application>(
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

            

            return GenericClassResponse<dynamic>.Create(appDictionary.Values, 0);
        }

        public async Task<dynamic> GetApplicationsByJob(int jobId)
        {

            // Step 1: Get recruiter profile by logged-in userId
            // Step 1: Get recruiter profile by logged-in userId
            var recruiter = await _db.QueryFirstOrDefaultAsync<dynamic>(
                                "SELECT Id FROM Profiles WHERE UserId = @UserId AND Type = 'recruiter'",
                                new { UserId = GetLoggedInUserId() }
                            );

            if (recruiter == null)
            {
                return new { data = recruiter, code = 1 };
            }

            // Step 2: Get job
            var job = await _db.QueryFirstOrDefaultAsync<dynamic>(
                            "SELECT Id, RecruiterId FROM Jobs WHERE Id = @JobId",
                            new { JobId = jobId }
                        );

            if (job == null)
            {
                return new { data = job, code = 2 };
            }

            // Step 3: Check if recruiter owns the job
            if (job.RecruiterId != recruiter.Id)
            {
                return new { data = job, code = 3 };
            }

            // Step 4: Get applications for the job with populated Job and Profile
            var sql = @"
                        SELECT 
                            a.*, 
                            j.Id, j.PositionTitle, j.Location, 
                            p.Id, p.FullName, p.Email
                        FROM Applications a
                        LEFT JOIN Jobs j ON a.JobId = j.Id
                        LEFT JOIN Profiles p ON a.ProfileId = p.Id
                        WHERE a.JobId = @JobId
                    ";

            var applications = await _db.QueryAsync<Application, Job, Profile, Application>(
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
            
            return new { data = applications, code = 0 };
            //return Ok(new { success = true, data = applications });
        }

        public async Task<dynamic> CreateApplication(int jobId, Application application)
        {

            var userId = 2;
            var job = await _db.QueryFirstOrDefaultAsync<Job>(
                                            "SELECT * FROM Jobs WHERE Id = @JobId", new { JobId = jobId });

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

        public async Task<ApplicationDetail?> GetApplicationWithJobAndProfileAsync(int applicationId)
        {
            using var multi = await _db.QueryMultipleAsync(@"
                                SELECT * FROM Applications WHERE Id = @Id;
                                SELECT p.* FROM Profiles p
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

    }
}
