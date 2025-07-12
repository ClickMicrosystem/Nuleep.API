using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Nuleep.Models;
using System.Data;
using Dapper;
using Azure.Core;
using Nuleep.Data.Interface;
using Nuleep.Models.Request;
using Nuleep.Models.Response;
using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Nuleep.Data.Repository
{
    public class AdminRepository : IAdminRepository
    {
        private readonly IDbConnection _db;

        public AdminRepository(IConfiguration config)
        {
            _db = new SqlConnection(config.GetConnectionString("DefaultConnection"));
        }

        public async Task<(IEnumerable<JobWithRecruiter>, int)> GetJobsByOrg(int orgId, int limit, int page)
        {

            var offset = (page - 1) * limit;

            var sql = @"
                        SELECT j.Id, j.Title, j.OrganizationId,
                               r.FirstName, r.LastName, r.Email
                        FROM Jobs j
                        INNER JOIN Recruiters r ON j.RecruiterId = r.Id
                        WHERE j.OrganizationId = @OrgId
                        ORDER BY j.Id DESC
                        OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY;

                        SELECT COUNT(*) FROM Jobs WHERE OrganizationId = @OrgId;
                    ";

            using var multi = await _db.QueryMultipleAsync(sql, new { OrgId = orgId, Offset = offset, Limit = limit });

            var jobs = await multi.ReadAsync<JobWithRecruiter>();
            var total = await multi.ReadFirstAsync<int>();

            return (jobs, total);
        }

        public async Task<(IEnumerable<EmployeeWithJobCount>, int)> GetEmployeeList(int orgId, bool isDelete, int limit, int page)
        {
            var offset = (page - 1) * limit;

            var sql = @"
                        SELECT r.Id, r.FirstName, r.LastName, COUNT(j.Id) AS JobCount
                        FROM Recruiters r
                        LEFT JOIN Jobs j ON j.RecruiterId = r.Id
                        WHERE r.OrganizationId = @OrgId AND r.IsDelete = @IsDelete
                        GROUP BY r.Id, r.FirstName, r.LastName
                        ORDER BY r.Id DESC
                        OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY;

                        SELECT COUNT(*) FROM Recruiters
                        WHERE OrganizationId = @OrgId AND IsDelete = @IsDelete;
                    ";

            using var multi = await _db.QueryMultipleAsync(sql, new
            {
                OrgId = orgId,
                IsDelete = isDelete,
                Offset = offset,
                Limit = limit
            });

            var employees = await multi.ReadAsync<EmployeeWithJobCount>();
            var total = await multi.ReadFirstAsync<int>();

            return (employees, total);
        }

        public async Task<Job?> EditJob(Job request)
        {
            var sql = @"
                        UPDATE Jobs
                        SET
                            Title = ISNULL(@Title, Title),
                            Description = ISNULL(@Description, Description),
                            Salary = ISNULL(@Salary, Salary),
                            ClosingDate = ISNULL(@ClosingDate, ClosingDate)
                        WHERE Id = @Id;

                        SELECT * FROM Jobs WHERE Id = @Id;
                    ";

            var result = await _db.QueryFirstOrDefaultAsync<Job>(sql, new
            {
                request.Id,
                request.PositionTitle,
                request.Description,
                request.Salary,
                request.ClosingDate
            });

            return result;
        }

        public async Task<bool> DeleteJob(int id)
        {
            var sql = "DELETE FROM Jobs WHERE Id = @Id";

            var rowsAffected = await _db.ExecuteAsync(sql, new { Id = id });

            return rowsAffected > 0;
        }

        public async Task<(IEnumerable<Organization>, int)> GetCompanyList(int limit, int page)
        {
            var offset = (page - 1) * limit;

            var sql = @"
                        SELECT * FROM Organizations
                        ORDER BY Id DESC
                        OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY;

                        SELECT COUNT(*) FROM Organizations;
                    ";

            using var multi = await _db.QueryMultipleAsync(sql, new { Offset = offset, Limit = limit });

            var list = await multi.ReadAsync<Organization>();
            var total = await multi.ReadFirstAsync<int>();

            return (list, total);
        }

        public async Task<int?> GetRecruiterIdByOrganization(int orgId)
        {
            var sql = "SELECT TOP 1 Id FROM Recruiters WHERE OrganizationId = @OrgId";

            return await _db.QueryFirstOrDefaultAsync<int?>(sql, new { OrgId = orgId });
        }

        public async Task<Job> InsertJob(Job job)
        {
            var sql = @"
                        INSERT INTO Jobs (NuleepId, PositionTitle, Description, Salary, OrganizationId, RecruiterId, ClosingDate, Location, Department, Requirements, SkillKeywords, JobType, SalaryType, Salary, Remote, RequisitionNumber, PostingDate, CompanyContact, CompanyEmail, RecruiterId)
                        VALUES (@NuleepId, @PositionTitle, @Description, @Salary, @OrganizationId, @RecruiterId, @ClosingDate, @Location, @Department, @Requirements, @SkillKeywords, @JobType, @SalaryType, @Salary, @Remote, @RequisitionNumber, @PostingDate, @CompanyContact, @CompanyEmail, @RecruiterId);
                        SELECT * FROM Jobs WHERE Id = SCOPE_IDENTITY();
                    ";

            return await _db.QueryFirstAsync<Job>(sql, new
            {
                PositionTitle = job.PositionTitle,
                Location = job.Location,
                Description = job.Description,
                Department = job.Department,
                Requirements = string.Join(',',job.Requirements),
                SkillKeywords = string.Join(',',job.SkillKeywords),
                JobType = job.JobType,
                SalaryType = job.SalaryType,
                Salary = job.Salary,
                Remote = job.Remote,
                RequisitionNumber = job.RequisitionNumber,
                PostingDate = job.PostingDate,
                ClosingDate = job.ClosingDate,
                CompanyContact = job.CompanyContact,
                CompanyEmail = job.CompanyEmail,
                NuleepID = job.NuleepID,
                OrganizationId = job.OrganizationId,
                RecruiterId = job.RecruiterId
            });
        }

        public async Task<Organization> GetCompanyById(int id)
        {
            var sql = "SELECT * FROM Organization WHERE Id = @Id";
            return await _db.QueryFirstOrDefaultAsync<Organization>(sql, new { Id = id });
        }

        public async Task<Organization> EditCompanyProfile(EditCompanyRequest request)
        {
            var sql = @"
                        UPDATE Organization
                        SET Name = @Name,
                            Email = @Email,
                            About = @About,
                            Culture = @Culture,
                            Mission = @Mission,
                            Benefits = @Benefits,
                            Perks = @Perks,
                            ZipPostal = @ZipPostal
                            City = @City
                        WHERE Id = @OrgId;

                        SELECT * FROM Organization WHERE Id = @OrgId;
                    ";

            var updated = await _db.QueryFirstOrDefaultAsync<Organization>(sql, new {
                Name = request.Name,
                Email = request.Email,
                About = request.About,
                Culture = request.Culture,
                Mission = request.Mission,
                Benefits = request.Benefits,
                Perks = request.Perks,
                ZipPostal = request.ZipPostal,
                City = request.City,
                OrgId = request.OrgId
            });
            return updated;
        }

        public async Task<Organization> CreateCompany(Organization organization)
        {
            string sql = @"
                            INSERT INTO Organizations (OrgCode, CompanyName, CompanyAbout, CompanyCulture, CompanyMission,
                                                       CompanyBenefits, CompanyPerks, CompanyEmail, CompanyStreet, ZipPostal)
                            VALUES (@OrgCode, @CompanyName, @CompanyAbout, @CompanyCulture, @CompanyMission,
                                    @CompanyBenefits, @CompanyPerks, @CompanyEmail, @CompanyStreet, @ZipPostal);
                            SELECT CAST(SCOPE_IDENTITY() as int);
                        ";

            var orgId = await _db.ExecuteScalarAsync<int>(sql, new
                            {
                                Name = organization.Name,
                                Email = organization.Email,
                                About = organization.About,
                                Culture = organization.Culture,
                                Mission = organization.Mission,
                                Benefits = organization.Benefits,
                                Perks = organization.Perks,
                                ZipPostal = organization.ZipPostal,
                                City = organization.City,
                            });

            User user = new User();
            user.Email = organization.Email;
            user.Role = "recruiter";
            user.Password = Guid.NewGuid().ToString("N").Substring(0, 8);

            var userId = await CreateUser(user);

            var recruiter = new Recruiter
            {
                UserId = userId,
                Email = organization.Email,
                FirstName = "Ad",
                LastName = "min",
                Type = "recruiter",
                OrganizationId = orgId,
                OrganizationRole = "admin",
                OrganizationApproved = true,
                
            };

            var recruiterId = await CreateRecruiter(recruiter);

            return await _db.ExecuteScalarAsync<Organization>("Select * from Organizations Where Id = @orgId", new { orgId = orgId });
        }

        public async Task<int> CreateUser(User user)
        {
            var sql = @"INSERT INTO Users (Email, Role, Password) 
                    VALUES (@Email, @Role, @Password); 
                    SELECT CAST(SCOPE_IDENTITY() as int);";

            return await _db.ExecuteScalarAsync<int>(sql, new
            {
                Email = user.Email,
                Role = user.Role,
                Password = user.Password
            });
        }

        public async Task<int> CreateRecruiter(Recruiter recruiter)
        {
            var profileQuery = @"INSERT INTO Profile (FirstName, LastName, Email, UserRef, Type, CreatedAt) 
                                VALUES (@FirstName, @LastName, @Email, @UserRef, @Type, @CreatedAt); 
                                SELECT CAST(SCOPE_IDENTITY() as int);";

            int profileId = await _db.ExecuteScalarAsync<int>(profileQuery, new
                            {
                                FirstName = recruiter.FirstName,
                                LastName = recruiter.LastName,
                                Email = recruiter.Email,
                                UserRef = recruiter.UserId,
                                Type = "recruiter",
                                CreatedAt = DateTime.UtcNow
                            });

            var recruiterQuery = @"INSERT INTO Recruiters (OrganizationRole, OrganizationApproved, ProfileId, CreatedAt) 
                                VALUES (@OrganizationRole, @OrganizationApproved, @ProfileId, @CreatedAt); 
                                SELECT CAST(SCOPE_IDENTITY() as int);";

            return await _db.ExecuteScalarAsync<int>(recruiterQuery, new
            {
                OrganizationRole = recruiter.OrganizationRole,
                OrganizationApproved = true,
                ProfileId = profileId,
                CreatedAt = DateTime.UtcNow
            });
        }

    }
}
