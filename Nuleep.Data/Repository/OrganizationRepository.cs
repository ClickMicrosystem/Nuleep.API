using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Nuleep.Models;
using System.Data;
using Dapper;
using Nuleep.Models.Blogs;
using Azure.Core;
using Nuleep.Data.Interface;

namespace Nuleep.Data.Repository
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly IDbConnection _db;

        public OrganizationRepository(IConfiguration config)
        {
            _db = new SqlConnection(config.GetConnectionString("DefaultConnection"));
        }

        private int GetLoggedInUserId()
        {
            return 2;
            //return int.Parse(User.FindFirst("id").Value); // assuming JWT has user id in claim "id"
        }


        public async Task<dynamic> GetEmployeeOrganization(int page, int limit)
        {

            var userId = GetLoggedInUserId();

            // 1. Find Recruiter Profile
            var profile = await _db.QueryFirstOrDefaultAsync<dynamic>(
                            "SELECT Id, OrganizationId, OrganizationApproved FROM Profiles WHERE UserId = @UserId AND Type = 'recruiter'",
                            new { UserId = userId }
                        );

            if (profile == null)
            {
                return (new { data = profile, code = 1 });                
            }

            // 2. Find Organization
            var organization = await _db.QueryFirstOrDefaultAsync<dynamic>(
                                "SELECT * FROM Organizations WHERE Id = @OrganizationId",
                                new { OrganizationId = profile.OrganizationId }
                            );

            if (organization == null)
            {
                return (new { data = organization, code = 2 });                
            }

            // 3. Check Profile Organization Status
            if (!profile.OrganizationApproved)
            {
                return (new { data = profile, code = 3 });               
            }

            // 4. Find All Org Jobs
            var jobs = (await _db.QueryAsync<dynamic>(@"
                            SELECT * FROM Jobs
                            WHERE OrganizationId = @OrganizationId
                            AND ClosingDate >= GETUTCDATE()
                        ", new { OrganizationId = organization.Id })).ToList();

                                var jobIds = jobs.Select(j => (int)j.Id).ToList();

                                // 5. Find All Applications for these jobs
                                var applications = (await _db.QueryAsync<dynamic>(@"
                            SELECT a.*, j.PositionTitle, p.FullName as ProfileName
                            FROM Applications a
                            INNER JOIN Jobs j ON a.JobId = j.Id
                            INNER JOIN Profiles p ON a.ProfileId = p.Id
                            WHERE a.JobId IN @JobIds
                        ", new { JobIds = jobIds })).ToList();

            // 6. Find Employees (with Pagination)
            var employees = (await _db.QueryAsync<dynamic>(@"
                                SELECT p.*, 
                                    (SELECT COUNT(*) FROM Jobs j WHERE j.RecruiterId = p.Id) as JobCount
                                FROM Profiles p
                                WHERE p.OrganizationId = @OrganizationId AND p.IsDeleted = 0
                                ORDER BY p.Id
                                OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY
                            ", new
                                    {
                                        OrganizationId = organization.Id,
                                        Offset = page * limit,
                                        Limit = limit
                                    })).ToList();


            //return (new { data = profile, code = 1 });

            // 7. Final response
            return (new
            {
                success = true,
                jobCount = jobs.Count,
                applicationCount = applications.Count,
                employeeCount = employees.Count,
                data = new
                {
                    jobs,
                    applications,
                    employees,
                    organization
                }
            });
        }
    }
}
