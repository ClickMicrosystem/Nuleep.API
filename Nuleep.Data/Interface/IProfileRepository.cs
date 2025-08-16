using Nuleep.Models;

namespace Nuleep.Data.Interface
{
    public interface IProfileRepository
    {
        Task<dynamic> GetUserByUsernameAsync(string username);
        Task<dynamic> ViewProfile(int profileId);
        Task RemoveResumeReferenceAsync(int id);
        Task SaveResumeAsync(int jobSeekerId, string fileName, string blobName, string fullUrl);
        Task<Profile> GetExistingProfileByUserAsync(string userId);
        Task<JobSeeker> GetJobSeekerProfileByUserId(string userId);
        Task<JobSeeker> CreateJobSeekerProfile(CreateOrUpdateProfileRequest profileRequest);
        Task<Recruiter> CreateRecruiterProfile(CreateOrUpdateProfileRequest profileRequest);
        Task<JobSeeker> UpdateJobSeekerProfile(CreateOrUpdateProfileRequest profileRequest);
        Task<Recruiter> UpdateRecruiterProfile(CreateOrUpdateProfileRequest profileRequest);
        Task<ProfileResponse> UpdateProfile(CreateOrUpdateProfileRequest profileRequest);
        Task<dynamic> DeleteProfile(int UserId);
        Task<JobSeeker> DeleteResumeAsync(MediaImage mediaImage);
        Task<JobSeeker> UpdateResumeAsync(int pId, MediaImage mediaImage);
        Task<dynamic> UpdateProfileImage(int profileId, MediaImage mediaImage);
        Task<dynamic> UpdateHeaderImage(int profileId, MediaImage mediaImage);
        Task<Recruiter> GetRecruiterProfileByUserId(string userId);
        Task<Recruiter?> GetAdminRecruiterProfileByOrgId(int orgId);

    }
}
