using Nuleep.Business.Interface;
using Nuleep.Data.Interface;
using Nuleep.Models;

namespace Nuleep.Business.Services
{
    public class ProfileService : IProfileService
    {
        
        private readonly IProfileRepository _profileRepository;

        public ProfileService(IProfileRepository profileRepository)
        {
            _profileRepository = profileRepository;
        }
        
        public async Task<dynamic> GetProfileByUsernameAsync(string username)
        {
            return await _profileRepository.GetUserByUsernameAsync(username);
        }
        
        public async Task<dynamic> ViewProfile(int profileId)
        {
            return await _profileRepository.ViewProfile(profileId);
        }
        
        public async Task RemoveResumeReferenceAsync(int id)
        {
            await _profileRepository.RemoveResumeReferenceAsync(id);
        }
        
        public async Task SaveResumeAsync(int jobSeekerId, string fileName, string blobName, string fullUrl)
        {
            await _profileRepository.SaveResumeAsync(jobSeekerId, fileName, blobName, fullUrl);
        }

        public async Task<Profile> GetExistingProfileByUserAsync(string userId)
        {
            return await _profileRepository.GetExistingProfileByUserAsync(userId);
        }

        public async Task<JobSeeker> CreateJobSeekerProfile(CreateOrUpdateProfileRequest profileRequest)
        {
            return await _profileRepository.CreateJobSeekerProfile(profileRequest);
        }

        public async Task<Recruiter> CreateRecruiterProfile(CreateOrUpdateProfileRequest profileRequest)
        {
            return await _profileRepository.CreateRecruiterProfile(profileRequest);
        }

        public async Task<JobSeeker> UpdateJobSeekerProfile(CreateOrUpdateProfileRequest profileRequest)
        {
            return await _profileRepository.UpdateJobSeekerProfile(profileRequest);
        }

        public async Task<Recruiter> UpdateRecruiterProfile(CreateOrUpdateProfileRequest profileRequest)
        {
            return await _profileRepository.UpdateRecruiterProfile(profileRequest);
        }

        public async Task<ProfileResponse> UpdateProfile(CreateOrUpdateProfileRequest profileRequest)
        {
            return await _profileRepository.UpdateProfile(profileRequest);
        }

        public async Task<dynamic> DeleteProfile(int UserId)
        {
            return await _profileRepository.DeleteProfile(UserId);
        }
    }
}
