using Nuleep.Business.Interface;
using Nuleep.Data.Interface;
using Nuleep.Models;
using Nuleep.Models.Response;

namespace Nuleep.Business.Services
{
    public class ApplicationService : IApplicationService
    {
        
        private readonly IApplicationRepository _applicationRepository;
        private readonly IProfileRepository _profileRepository;
        private readonly IJobRepository _jobRepository;

        public ApplicationService(IApplicationRepository applicationRepository, IProfileRepository profileRepository, IJobRepository jobRepository)
        {
            _applicationRepository = applicationRepository;
            _profileRepository = profileRepository;
            _jobRepository = jobRepository;
        }
        
        public async Task<dynamic> GetAllRecruiterApplications(string username)
        {
            return await _applicationRepository.GetAllRecruiterApplications(username);
        }
        
        public async Task<dynamic> GetAllJobSeekerApplications(string username)
        {
            return await _applicationRepository.GetAllJobSeekerApplications(username);
        }
        
        public async Task<dynamic> GetApplicationsByJob(int jobId, int userId)
        {
            return await _applicationRepository.GetApplicationsByJob(jobId, userId);
        }
        
        public async Task<dynamic> CreateApplication(int jobId, Application application, int userId)
        {
            return await _applicationRepository.CreateApplication(jobId, application, userId);
        }

        public async Task<ApplicationDetail?> GetApplicationById(int applicationId, int userId)
        {
            var app = await _applicationRepository.GetApplicationById(applicationId);
            if (app == null) return null;

            var recruiter = await _profileRepository.GetRecruiterProfileByUserId(userId.ToString());
            var jobSeeker = await _profileRepository.GetJobSeekerProfileByUserId(userId.ToString());

            if (recruiter == null && jobSeeker == null)
                return null;

            // Authorization check
            if (jobSeeker != null && app.Profile.Id != jobSeeker.Id)
                return null;

            if (recruiter != null && app.Job.OrganizationId != recruiter.OrganizationId)
                return null;

            return app;
        }

        public async Task<ApplicationDetail?> UpdateApplication(int applicationId, int userId, Application request)
        {
            var application = await _applicationRepository.GetApplicationById(applicationId);
            if (application == null) return null;

            var job = await _jobRepository.GetJobById(application.Job.Id);
            if (job == null) return null;

            var recruiter = await _profileRepository.GetRecruiterProfileByUserId(userId.ToString());
            if (recruiter == null || job.RecruiterId != recruiter.Id)
                return null;

            var updated = await _applicationRepository.UpdateApplication(applicationId, request);
            return updated;
        }

        public async Task<bool> DeleteApplication(int applicationId, int userId)
        {
            var application = await _applicationRepository.GetApplicationById(applicationId);
            if (application == null) return false;

            var jobSeekerProfile = await _profileRepository.GetJobSeekerProfileByUserId(userId.ToString());
            if (jobSeekerProfile == null || application.Profile.Id != jobSeekerProfile.Id)
                return false;

            return await _applicationRepository.DeleteApplication(applicationId);
        }



    }
}
