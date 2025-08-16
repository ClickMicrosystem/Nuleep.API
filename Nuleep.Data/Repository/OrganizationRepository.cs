using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Nuleep.Models;
using System.Data;
using Dapper;
using Nuleep.Models.Blogs;
using Azure.Core;
using Nuleep.Data.Interface;
using Nuleep.Models.Response;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Text.RegularExpressions;

namespace Nuleep.Data.Repository
{
    public class OrganizationRepository : IOrganizationRepository
    {
        private readonly IDbConnection _db;

        public OrganizationRepository(IConfiguration config)
        {
            _db = new SqlConnection(config.GetConnectionString("DefaultConnection"));
        }

        public async Task<Organization?> GetOrgByOrgCode(string orgCode)
        {
            var sql = "SELECT * FROM Organizations WHERE OrgCode = @OrgCode";
            return await _db.QueryFirstOrDefaultAsync<Organization>(sql, new { OrgCode = orgCode });
        }

        public async Task<ResponeModel> GetEmployeeOrganization(int page, int limit, string userId)
        {
            ResponeModel responeModel = new ResponeModel();

            // 1. Find Recruiter Profile
            var profile = await _db.QueryFirstOrDefaultAsync<dynamic>(
                            "SELECT Id, OrganizationId, OrganizationApproved FROM Profile WHERE UserId = @UserId AND Type = 'recruiter'",
                            new { UserId = userId }
                        );

            if (profile == null)
            {
                responeModel.code = 1;
                return responeModel;
            }

            // 2. Find Organization
            var organization = await _db.QueryFirstOrDefaultAsync<Organization>(
                                "SELECT * FROM Organizations WHERE Id = @OrganizationId",
                                new { OrganizationId = profile.OrganizationId }
                            );

            if (organization == null)
            {
                responeModel.code = 2;
                return responeModel;
            }

            // 3. Check Profile Organization Status
            if (!profile.OrganizationApproved)
            {
                responeModel.code = 3;
                return responeModel;
            }

            // 4. Find All Org Jobs
            var jobs = (await _db.QueryAsync<dynamic>(@"
                            SELECT * FROM Jobs
                            WHERE OrganizationId = @OrganizationId
                            AND ClosingDate >= GETUTCDATE()
                        ", new { OrganizationId = organization.Id })).ToList();

            var jobIds = jobs.Select(j => (int)j.Id).ToList();

            var applications = (await _db.QueryAsync<dynamic>(@"
                                    SELECT a.*, j.PositionTitle, p.FullName as ProfileName
                                    FROM Applications a
                                    INNER JOIN Jobs j ON a.JobId = j.Id
                                    INNER JOIN Profile p ON a.ProfileId = p.Id
                                    WHERE a.JobId IN @JobIds
                                ", new { JobIds = jobIds })).ToList();

            var employees = (await _db.QueryAsync<dynamic>(@"
                                SELECT p.*, 
                                    (SELECT COUNT(*) FROM Jobs j WHERE j.RecruiterId = p.Id) as JobCount
                                FROM Profile p
                                WHERE p.OrganizationId = @OrganizationId AND p.IsDeleted = 0
                                ORDER BY p.Id
                                OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY
                            ", new
                                    {
                                        OrganizationId = organization.Id,
                                        Offset = page * limit,
                                        Limit = limit
                                    })).ToList();


            responeModel.data = new
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
                                };

            return responeModel;
        }

        public async Task<OrganizationsResponse> GetOrganizationById(int orgId)
        {
            OrganizationsResponse organizationsResponse = new OrganizationsResponse();
            var org =  await _db.QueryFirstOrDefaultAsync<Organization>("SELECT * FROM Organizations WHERE Id = @Id", new { Id = orgId });

            if(org?.Id > 0)
            {
                organizationsResponse.Id = org.Id;
                organizationsResponse.benefits = org.Benefits?.Split(',').ToList();
                organizationsResponse.perks = org.Perks?.Split(',').ToList();
                organizationsResponse.sendOwenerShip = org.SendOwnership;
                organizationsResponse.about = org.About;
                organizationsResponse.streetAddress = org.StreetAddress;
                organizationsResponse.countryRegion = org.CountryRegion;
                organizationsResponse.stateProvince = org.StateProvince;
                organizationsResponse.zipPostal = org.ZipPostal;
                organizationsResponse.city = org.City;
                organizationsResponse.tel = org.Tel;
                organizationsResponse.culture = org.Culture;
                organizationsResponse.mission = org.Mission;
                organizationsResponse.teamSize = org.TeamSize;
                organizationsResponse.verified = org.Verified;
                organizationsResponse.orgImage.FileName = org.OrgImageFileName;
                organizationsResponse.orgImage.BlobName = org.OrgImageBlobName;
                organizationsResponse.orgImage.FullUrl = org.OrgImageFullUrl;
            }

            return organizationsResponse;

        }

        public async Task<List<Job>> GetJobsByOrganizationId(int orgId)
        {
            var sql = "SELECT * FROM Jobs WHERE OrganizationId = @OrgId";
            var result = await _db.QueryAsync<Job>(sql, new { OrgId = orgId });
            return result.ToList();
        }

        public async Task<int> CreateOrganization(Organization org)
        {
            var sql = @"INSERT INTO Organizations (Name, About, StreetAddress, CountryRegion, StateProvince, ZipPostal, City, Email, Culture, Mission, Benefits, Perks) 
                    VALUES (@Name, @About, @StreetAddress, @CountryRegion, @StateProvince, @ZipPostal, @City, @Email, @Culture, @Mission, @Benefits, Perks);
                    SELECT CAST(SCOPE_IDENTITY() as int);";
            return await _db.ExecuteScalarAsync<int>(sql, new
            {
                Name = org.Name,
                About = org.About,
                StreetAddress = org.StreetAddress,
                CountryRegion = org.CountryRegion,
                StateProvince = org.StateProvince,
                ZipPostal = org.ZipPostal,
                City = org.City,
                Email = org.Email,
                Culture = org.Culture,
                Mission = org.Mission,
                Benefits = org.Benefits,
                Perks = org.Perks
            });
        }

        public async Task UpdateRecruiter(Recruiter recruiter)
        {
            var sql = @"UPDATE Recruiters 
                    SET OrganizationRole = @OrganizationRole, 
                        OrganizationApproved = @OrganizationApproved
                    WHERE ProfileId = @ProfileId";
            await _db.ExecuteAsync(sql, new
            {
                OrganizationRole = recruiter.OrganizationRole,
                OrganizationApproved = recruiter.OrganizationApproved,
                ProfileId = recruiter.Id
            });
        }

        public async Task EditOrganization(int orgId, Organization org)
        {
            var sql = @"
            UPDATE Organizations
            SET Name = @Name,
                About = @About,
                StreetAddress = @StreetAddress,
                CountryRegion = @CountryRegion,
                StateProvince = @StateProvince,
                ZipPostal = @ZipPostal,
                City = @City,
                Email = @Email,
                Culture = @Culture,
                Mission = @Mission,
                Benefits = @Benefits,
                Perks = @Perks
            WHERE OrganizationId = @OrgId";

            await _db.ExecuteAsync(sql, new
            {
                Name = org.Name,
                About = org.About,
                StreetAddress = org.StreetAddress,
                CountryRegion = org.CountryRegion,
                StateProvince = org.StateProvince,
                ZipPostal = org.ZipPostal,
                City = org.City,
                Email = org.Email,
                Culture = org.Culture,
                Mission = org.Mission,
                Benefits = org.Benefits,
                Perks = org.Perks,
                OrgId = org.Id
            });
        }

        public async Task<bool> UpdateOrganizationLogo(int orgId, MediaImage orgImage)
        {
            var sql = @"UPDATE Organizations
                    SET OrgImageFileName = @OrgImageFileName,
                        OrgImageBlobName = @OrgImageBlobName,
                        OrgImageFullUrl = @OrgImageFullUrl
                    WHERE Id = @Id";
            var rows = await _db.ExecuteAsync(sql, new
            {
                Id = orgId,
                OrgImageFileName = orgImage.FileName,
                OrgImageBlobName = orgImage.BlobName,
                OrgImageFullUrl = orgImage.FullUrl
            });
            return rows > 0;
        }

        public async Task<List<OrganizationsResponse>> GetAllOrganizationList()
        {
            List<OrganizationsResponse> results = new List<OrganizationsResponse>();

            var sql = "SELECT Id FROM Organizations";
            var ids = (await _db.QueryAsync<int>(sql)).ToList();

            foreach (var id in ids)
            {
                results.Add(await this.GetOrganizationById(id));
            }

            return results;
        }

        public async Task UpdateRecruiterProfileOrg(int profileId, int orgId)
        {
            var sql = @"UPDATE Recruiters 
                    SET OrganizationId = @OrgId, 
                        OrganizationRole = 'unapproved', 
                        OrganizationApproved = 0 
                    WHERE ProfileId = @ProfileId";
            await _db.ExecuteAsync(sql, new { ProfileId = profileId, OrgId = orgId });
        }

        public async Task UpdateProfileApproveOrg(int profileId, int orgId, string role)
        {
            var sql = @"UPDATE Recruiters 
                    SET OrganizationId = @OrganizationId,
                        OrganizationRole = @OrganizationRole,
                        OrganizationApproved = @OrganizationApproved
                    WHERE ProfileId = @ProfileId";
            await _db.ExecuteAsync(sql, new
            {
                ProfileId = profileId,
                OrganizationId = orgId,
                OrganizationRole = role
            });
        }

    }
}
