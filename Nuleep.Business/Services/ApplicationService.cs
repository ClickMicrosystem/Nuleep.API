using Nuleep.Business.Interface;
using Nuleep.Data.Interface;
using Nuleep.Models;

namespace Nuleep.Business.Services
{
    public class ApplicationService : IApplicationService
    {
        
        private readonly IApplicationRepository _applicationRepository;

        public ApplicationService(IApplicationRepository applicationRepository)
        {
            _applicationRepository = applicationRepository;
        }
        
        public async Task<dynamic> GetAllRecruiterApplications(string username)
        {
            return await _applicationRepository.GetAllRecruiterApplications(username);
        }
        
        public async Task<dynamic> GetApplicationsByJob(int jobId)
        {
            return await _applicationRepository.GetApplicationsByJob(jobId);
        }
        
        public async Task<dynamic> CreateApplication(int jobId, Application application)
        {
            return await _applicationRepository.GetApplicationsByJob(jobId);
        }
        
    }
}
