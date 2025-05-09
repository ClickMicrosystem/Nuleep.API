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

        public async Task<Profile> GetExistingProfileByUserAsync(string userId)
        {
            return await _profileRepository.GetExistingProfileByUserAsync(userId);
        }

        public async Task<dynamic> CreateProfile(CreateProfileRequest profileRequest)
        {
            return await _profileRepository.CreateProfile(profileRequest);
        }

        public async Task<dynamic> UpdateProfile(CreateProfileRequest profileRequest)
        {
            return await _profileRepository.UpdateProfile(profileRequest);
        }

        public async Task<dynamic> DeleteProfile(int UserId)
        {
            return await _profileRepository.DeleteProfile(UserId);
        }
    }
}
