using Nuleep.Models;

namespace Nuleep.Data.Interface
{
    public interface IProfileRepository
    {
        Task<dynamic> GetUserByUsernameAsync(string username);
        Task<Profile> GetExistingProfileByUserAsync(string userId);
        Task<dynamic> CreateProfile(CreateProfileRequest profileRequest);
        Task<dynamic> UpdateProfile(CreateProfileRequest profileRequest);
        Task<dynamic> DeleteProfile(int UserId);

    }
}
