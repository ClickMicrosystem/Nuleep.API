using Nuleep.Models;
using Nuleep.Models.Request;
using Nuleep.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Business.Interface
{
    public interface IAdminService
    {
        Task<(IEnumerable<JobWithRecruiter>, int)> GetJobsByOrg(int orgId, int limit, int page);
        Task<(IEnumerable<EmployeeWithJobCount>, int)> GetEmployeeList(int orgId, bool isDelete, int limit, int page);
        Task<Job?> EditJob(Job request);
        Task<bool> DeleteJob(int id);
        Task<(IEnumerable<Organization>, int)> GetCompanyList(int limit, int page);
        Task<int?> GetRecruiterIdByOrganization(int orgId);
        Task<Job> InsertJob(Job job);
        Task<Organization> GetCompanyById(int id);
        Task<Organization> EditCompanyProfile(EditCompanyRequest request);
        Task<Organization> CreateCompany(CreateCompanyRequest request);
    }
}
