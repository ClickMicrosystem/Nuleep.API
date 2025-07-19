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

        public ApplicationService(IApplicationRepository applicationRepository, IProfileRepository profileRepository)
        {
            _applicationRepository = applicationRepository;
            _profileRepository = profileRepository;
        }
        
        public async Task<dynamic> GetAllRecruiterApplications(string username)
        {
            return await _applicationRepository.GetAllRecruiterApplications(username);
        }
        
        public async Task<dynamic> GetAllJobSeekerApplications(string username)
        {
            return await _applicationRepository.GetAllJobSeekerApplications(username);
        }
        
        public async Task<dynamic> GetApplicationsByJob(int jobId)
        {
            return await _applicationRepository.GetApplicationsByJob(jobId);
        }
        
        public async Task<dynamic> CreateApplication(int jobId, Application application)
        {
            return await _applicationRepository.GetApplicationsByJob(jobId);
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


    }
}
