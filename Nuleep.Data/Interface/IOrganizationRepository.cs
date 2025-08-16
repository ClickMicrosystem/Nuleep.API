using Nuleep.Models;
using Nuleep.Models.Blogs;
using Nuleep.Models.Response;

namespace Nuleep.Data
{
    public interface IOrganizationRepository
    {
        Task<ResponeModel> GetEmployeeOrganization(int page, int limit, string userId);
        Task<Organization?> GetOrgByOrgCode(string orgCode);
        Task<OrganizationsResponse> GetOrganizationById(int orgId);
        Task<List<Job>> GetJobsByOrganizationId(int orgId);
        Task<int> CreateOrganization(Organization org);
        Task UpdateRecruiter(Recruiter recruiter);
        Task EditOrganization(int orgId, Organization org);
        Task<bool> UpdateOrganizationLogo(int orgId, MediaImage orgImage);
        Task<List<OrganizationsResponse>> GetAllOrganizationList();
        Task UpdateRecruiterProfileOrg(int profileId, int orgId);
        Task UpdateProfileApproveOrg(int profileId, int orgId, string role);
    }
}
