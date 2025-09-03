using Nuleep.Models;
using Nuleep.Models.Request;
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
        Task<dynamic> ViewProfile(int profileId);
        Task RemoveResumeReferenceAsync(int id);
        Task SaveResumeAsync(int jobSeekerId, string fileName, string blobName, string fullUrl);
        Task<Profile> GetExistingProfileByUserAsync(string userId);
        Task<JobSeeker> CreateJobSeekerProfile(CreateOrUpdateProfileRequest profileRequest);
        Task<Recruiter> CreateRecruiterProfile(CreateOrUpdateProfileRequest profileRequest);
        Task<JobSeeker> UpdateJobSeekerProfile(CreateOrUpdateProfileRequest profileRequest);
        Task<Recruiter> UpdateRecruiterProfile(CreateOrUpdateProfileRequest profileRequest);
        Task<ProfileResponse> UpdateProfile(CreateOrUpdateProfileRequest profileRequest);
        Task<dynamic> DeleteProfile(int UserId);
        Task<dynamic> UpdateProfileImage(int profileId, MediaImage mediaImage);
        Task<dynamic> UpdateHeaderImage(int profileId, MediaImage mediaImage);
        Task<int> JoinChatProfile(JoinChatProfileRequest request);
        Task<(IEnumerable<CandidateSummary> Data, int Total)> SearchCandidates(SearchCandidatesRequest dto);

    }
}
