using Nuleep.Business.Interface;
using Nuleep.Data;
using Nuleep.Data.Interface;
using Nuleep.Data.Repository;
using Nuleep.Models;
using Nuleep.Models.Blogs;
using Nuleep.Models.Response;

namespace Nuleep.Business.Services
{
    public class OrganizationService : IOrganizationService
    {
        
        private readonly IOrganizationRepository _organizationRepository;

        public OrganizationService(IOrganizationRepository organizationRepository)
        {
            _organizationRepository = organizationRepository;
        }
        public async Task<ResponeModel> GetEmployeeOrganization(int page, int limit, string userId)
        {
            return await _organizationRepository.GetEmployeeOrganization(page, limit, userId);
        }
        public async Task<Organization> GetByOrgCode(string orgCode)
        {
            return await _organizationRepository.GetByOrgCode(orgCode);
        }
        public async Task<OrganizationsResponse> GetOrganizationById(int orgId)
        {
            return await _organizationRepository.GetOrganizationById(orgId);
        }
        public async Task<List<Job>> GetJobsByOrganizationId(int orgId)
        {
            return await _organizationRepository.GetJobsByOrganizationId(orgId);
        }
    }
}
