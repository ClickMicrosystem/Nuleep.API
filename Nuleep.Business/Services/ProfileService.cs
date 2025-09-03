using Newtonsoft.Json;
using System.Net.Http.Headers;
using Nuleep.Business.Interface;
using Nuleep.Data.Interface;
using Nuleep.Models;
using Nuleep.Models.Request;

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

        public async Task<dynamic> UpdateProfileImage(int profileId, MediaImage mediaImage)
        {
            return await _profileRepository.UpdateProfileImage(profileId, mediaImage);
        }

        public async Task<dynamic> UpdateHeaderImage(int profileId, MediaImage mediaImage)
        {
            return await _profileRepository.UpdateHeaderImage(profileId, mediaImage);
        }

        public async Task<int> JoinChatProfile(JoinChatProfileRequest request)
        {
            string roomName = $"{request.JobSeek.Email}-join-{request.JobReq.Email}";

            var existingRoom = await _profileRepository.GetChatRoomByName(roomName);
            if (existingRoom != null)
            {
                return existingRoom.Id??0;
            }

            // Create new room
            var room = new ChatRoom { Name = roomName };
            int roomId = await _profileRepository.CreateChatRoom(room);

            // Add users
            await _profileRepository.AddUserToChatRoom(roomId, request.JobSeek.Id);
            await _profileRepository.AddUserToChatRoom(roomId, request.JobReq.Id);

            return roomId;
        }

        public async Task<(IEnumerable<CandidateSummary> Data, int Total)> SearchCandidates(SearchCandidatesRequest dto)
        {
            var results = await _profileRepository.SearchCandidates(dto.Name, dto.Limit, dto.Page);
            return (results, results.Count());
        }
    }
}
