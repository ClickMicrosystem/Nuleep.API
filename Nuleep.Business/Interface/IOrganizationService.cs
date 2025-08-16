using Microsoft.AspNetCore.Http;
using Nuleep.Models;
using Nuleep.Models.Blogs;
using Nuleep.Models.Request;
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
        Task<(bool Success, string ErrorMessage, OrganizationsResponse? Data)> CreateOrganization(string userId, Organization request);
        Task<OrganizationsResponse> EditOrganization(int orgId, string userId, Organization dto);
        Task<OrganizationsResponse> UpdateOrganizationLogo(int orgId, MediaImage file);
        Task<List<OrganizationsResponse>> GetAllOrganizationList();
        Task JoinOrganization(JoinOrganizationRequest request);
        Task ApproveJoinOrganization(int profileId, int orgId, int currentUserId, string role);
    }
}
