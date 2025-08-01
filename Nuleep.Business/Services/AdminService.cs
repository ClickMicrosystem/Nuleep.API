using Azure.Core;
using Nuleep.Business.Interface;
using Nuleep.Data.Interface;
using Nuleep.Models;
using Nuleep.Models.Request;
using Nuleep.Models.Response;

namespace Nuleep.Business.Services
{
    public class AdminService : IAdminService
    {
        
        private readonly IAdminRepository _adminRepository;

        public AdminService(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
        }

        public async Task<(IEnumerable<JobWithRecruiter>, int)> GetJobsByOrg(int orgId, int limit, int page)
        {
            return await _adminRepository.GetJobsByOrg(orgId, limit, page);
        }

        public async Task<(IEnumerable<EmployeeWithJobCount>, int)> GetEmployeeList(int orgId, bool isDelete, int limit, int page)
        {
            return await _adminRepository.GetEmployeeList(orgId, isDelete, limit, page);
        }

        public async Task<Job?> EditJob(Job request)
        {
            return await _adminRepository.EditJob(request);
        }

        public async Task<bool> DeleteJob(int id)
        {
            return await _adminRepository.DeleteJob(id);
        }

        public async Task<(IEnumerable<Organization>, int)> GetCompanyList(int limit, int page)
        {
            return await _adminRepository.GetCompanyList(limit, page);
        }

        public async Task<int?> GetRecruiterIdByOrganization(int orgId)
        {
            return await _adminRepository.GetRecruiterIdByOrganization(orgId);
        }

        public async Task<Job> InsertJob(Job job)
        {
            return await _adminRepository.InsertJob(job);
        }

        public async Task<Organization> GetCompanyById(int id)
        {
            return await _adminRepository.GetCompanyById(id);
        }

        public async Task<Organization> EditCompanyProfile(EditCompanyRequest request)
        {
            return await _adminRepository.EditCompanyProfile(request);
        }

        public async Task<Organization> CreateCompany(CreateCompanyRequest request)
        {
            var organization = new Organization
            {
                OrgCode = $"nuleep-{Guid.NewGuid().ToString("N").Substring(0, 8)}",
                Name = request.CompanyName,
                About = request.CompanyAbout,
                Culture = request.CompanyCulture,
                Mission = request.CompanyMission,
                Benefits = string.Join(",", request.CompanyBenefits),
                Perks = string.Join(",", request.CompanyPerks),
                Email = request.CompanyEmail,
                StreetAddress = request.CompanyStreet,
                ZipPostal = request.ZipPostal
            };

            return await _adminRepository.CreateCompany(organization);
        }

    }
}
