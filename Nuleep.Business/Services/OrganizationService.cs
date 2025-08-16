using Microsoft.AspNetCore.Http;
using Nuleep.Business.Interface;
using Nuleep.Data;
using Nuleep.Data.Interface;
using Nuleep.Data.Repository;
using Nuleep.Models;
using Nuleep.Models.Blogs;
using Nuleep.Models.Request;
using Nuleep.Models.Response;

namespace Nuleep.Business.Services
{
    public class OrganizationService : IOrganizationService
    {
        
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IProfileRepository _profileRepository;
        private readonly INotificationsRepository _notificationsRepository;

        public OrganizationService(IOrganizationRepository organizationRepository, IProfileRepository profileRepository, INotificationsRepository notificationsRepository)
        {
            _organizationRepository = organizationRepository;
            _profileRepository = profileRepository;
            _notificationsRepository = notificationsRepository;
        }
        
        public async Task<ResponeModel> GetEmployeeOrganization(int page, int limit, string userId)
        {
            return await _organizationRepository.GetEmployeeOrganization(page, limit, userId);
        }
        
        public async Task<Organization> GetByOrgCode(string orgCode)
        {
            return await _organizationRepository.GetOrgByOrgCode(orgCode);
        }
        
        public async Task<OrganizationsResponse> GetOrganizationById(int orgId)
        {
            return await _organizationRepository.GetOrganizationById(orgId);
        }
        
        public async Task<List<Job>> GetJobsByOrganizationId(int orgId)
        {
            return await _organizationRepository.GetJobsByOrganizationId(orgId);
        }

        public async Task<(bool Success, string ErrorMessage, OrganizationsResponse? Data)> CreateOrganization(string userId, Organization request)
        {
            var recruiter = await _profileRepository.GetRecruiterProfileByUserId(userId);

            if (recruiter == null)
                return (false, "Recruiter profile not found", null);

            if (recruiter.OrganizationId.HasValue)
            {
                var existingOrg = await _organizationRepository.GetOrganizationById(recruiter.OrganizationId.Value);
                if (existingOrg != null)
                    return (false, "You have created an organization already!", null);
            }

            request.OrgCode = "nuleep-" + Guid.NewGuid().ToString("N").Substring(0, 8);
            request.Verified = true;

            var orgId = await _organizationRepository.CreateOrganization(request);

            recruiter.OrganizationRole = "admin";
            recruiter.OrganizationId = orgId;
            recruiter.OrganizationApproved = true;

            await _organizationRepository.UpdateRecruiter(recruiter);

            return (true, "", await _organizationRepository.GetOrganizationById(orgId));
        }

        public async Task<OrganizationsResponse> EditOrganization(int orgId, string userId, Organization orgRequest)
        {
            var recruiter = await _profileRepository.GetRecruiterProfileByUserId(userId);

            if (recruiter == null)
                throw new UnauthorizedAccessException("Recruiter profile not found.");

            if (!string.Equals(recruiter.OrganizationRole, "admin", StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Not authorized to edit organization.");

            var organization = await _organizationRepository.GetOrganizationById(orgId);

            if (organization == null)
                throw new KeyNotFoundException($"Organization not found with id {orgId}.");

            if (recruiter.OrganizationId != organization.Id)
                throw new UnauthorizedAccessException("You are not authorized to edit this organization.");

            await _organizationRepository.EditOrganization(orgId, orgRequest);

            return await _organizationRepository.GetOrganizationById(orgId);
        }

        public async Task<OrganizationsResponse> UpdateOrganizationLogo(int orgId, MediaImage file)
        {
            await _organizationRepository.UpdateOrganizationLogo(orgId, file);

            return await _organizationRepository.GetOrganizationById(orgId);
        }

        public async Task<List<OrganizationsResponse>> GetAllOrganizationList()
        {
            return await _organizationRepository.GetAllOrganizationList();
        }

        public async Task JoinOrganization(JoinOrganizationRequest request)
        {
            // 1. Check org exists
            var organization = await _organizationRepository.GetOrgByOrgCode(request.OrgCode);
            if (organization == null)
                throw new KeyNotFoundException("Organization does not exist");

            // 2. Get profile
            var profile = await _profileRepository.GetRecruiterProfileByUserId(request.UserId.ToString());
            if (profile == null)
                throw new KeyNotFoundException("Profile does not exist");

            // 3. Ensure not already tied
            if (profile.OrganizationId != null)
                throw new InvalidOperationException("Profile already tied to another organization");

            // 4. Get admin of org & notify
            Recruiter? adminRecProfile = await _profileRepository.GetAdminRecruiterProfileByOrgId(request.OrgId);
            if (adminRecProfile != null)
            {
                var notification = new Notification
                {
                    Title = $"<span class='menuList'>New user <b>{profile.FullName}</b> join your ORG",
                    UserId = adminRecProfile.User?.Id.ToString(),
                    NotificationType = "ORGJOIN"
                };
                await _notificationsRepository.AddNotification(notification);
            }

            // 5. Update profile
            await _organizationRepository.UpdateRecruiterProfileOrg(profile.Id, organization.Id);
        }

        public async Task ApproveJoinOrganization(int profileId, int orgId, int currentUserId, string role)
        {
            var requestingProfile = await _profileRepository.ViewProfile(profileId);

            if (requestingProfile == null || requestingProfile?.OrganizationId != orgId)
                throw new Exception("Profile does not exist or is not tied to this organization");

            var adminProfile = await _profileRepository.GetRecruiterProfileByUserId(currentUserId.ToString());

            if (adminProfile == null || adminProfile.OrganizationId != orgId || adminProfile.OrganizationRole != "admin")
                throw new Exception("You are not authorized to approve this profile");

            // Approve
            requestingProfile.OrganizationRole = role;
            requestingProfile.OrganizationApproved = true;
            requestingProfile.OrganizationId = orgId;

            await _organizationRepository.UpdateProfileApproveOrg(profileId, orgId, role);
        }
    }
}
