using Nuleep.Models;
using Nuleep.Models.Blogs;

namespace Nuleep.Data
{
    public interface IOrganizationRepository
    {
        Task<dynamic> GetEmployeeOrganization(int page, int limit);
        Task<Organization> GetByOrgCode(string orgCode);
    }
}
