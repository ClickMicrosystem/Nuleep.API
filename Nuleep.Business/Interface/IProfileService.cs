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
        Task<JobSeeker> CreateJobSeekerProfile(CreateOrUpdateProfileRequest profileRequest);
        Task<Recruiter> CreateRecruiterProfile(CreateOrUpdateProfileRequest profileRequest);
        Task<JobSeeker> UpdateJobSeekerProfile(CreateOrUpdateProfileRequest profileRequest);
        Task<Recruiter> UpdateRecruiterProfile(CreateOrUpdateProfileRequest profileRequest);
        Task<ProfileResponse> UpdateProfile(CreateOrUpdateProfileRequest profileRequest);
        Task<dynamic> DeleteProfile(int UserId);

    }
}
