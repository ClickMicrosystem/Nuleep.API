using Nuleep.Models;
using Nuleep.Models.Blogs;
using Nuleep.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Business.Interface
{
    public interface IOrganizationService
    {
        Task<ResponeModel> GetEmployeeOrganization(int page, int limit, string userId);
        Task<Organization> GetByOrgCode(string orgCode);
        Task<OrganizationsResponse> GetOrganizationById(int orgId);
        Task<List<Job>> GetJobsByOrganizationId(int orgId);
    }
}
