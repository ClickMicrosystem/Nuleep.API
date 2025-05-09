using Nuleep.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Business.Interface
{
    public interface IProfileService
    {
        Task<dynamic> GetProfileByUsernameAsync(string username);
        Task<Profile> GetExistingProfileByUserAsync(string userId);
        Task<dynamic> CreateProfile(CreateProfileRequest profileRequest);
        Task<dynamic> UpdateProfile(CreateProfileRequest profileRequest);
        Task<dynamic> DeleteProfile(int UserId);

    }
}
