using Nuleep.Models;

namespace Nuleep.Data.Interface
{
    public interface IProfileRepository
    {
        Task<dynamic> GetUserByUsernameAsync(string username);
        Task<Profile> GetExistingProfileByUserAsync(string userId);
        Task<JobSeeker> CreateJobSeekerProfile(CreateOrUpdateProfileRequest profileRequest);
        Task<Recruiter> CreateRecruiterProfile(CreateOrUpdateProfileRequest profileRequest);
        Task<JobSeeker> UpdateJobSeekerProfile(CreateOrUpdateProfileRequest profileRequest);
        Task<Recruiter> UpdateRecruiterProfile(CreateOrUpdateProfileRequest profileRequest);
        Task<ProfileResponse> UpdateProfile(CreateOrUpdateProfileRequest profileRequest);
        Task<dynamic> DeleteProfile(int UserId);

    }
}
