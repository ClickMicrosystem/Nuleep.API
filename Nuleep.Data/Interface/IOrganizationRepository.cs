using Nuleep.Models;
using Nuleep.Models.Blogs;
using Nuleep.Models.Response;

namespace Nuleep.Data
{
    public interface IOrganizationRepository
    {
        Task<ResponeModel> GetEmployeeOrganization(int page, int limit, string userId);
        Task<Organization> GetByOrgCode(string orgCode);
        Task<OrganizationsResponse> GetOrganizationById(int orgId);
        Task<List<Job>> GetJobsByOrganizationId(int orgId);
    }
}
