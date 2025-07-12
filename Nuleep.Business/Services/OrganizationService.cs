using Nuleep.Business.Interface;
using Nuleep.Data;
using Nuleep.Data.Interface;
using Nuleep.Models;
using Nuleep.Models.Blogs;

namespace Nuleep.Business.Services
{
    public class OrganizationService : IOrganizationService
    {
        
        private readonly IOrganizationRepository _organizationRepository;

        public OrganizationService(IOrganizationRepository organizationRepository)
        {
            _organizationRepository = organizationRepository;
        }
        public async Task<dynamic> GetEmployeeOrganization(int page, int limit)
        {
            return await _organizationRepository.GetEmployeeOrganization(page, limit);
        }
        public async Task<Organization> GetByOrgCode(string orgCode)
        {
            return await _organizationRepository.GetByOrgCode(orgCode);
        }
    }
}
