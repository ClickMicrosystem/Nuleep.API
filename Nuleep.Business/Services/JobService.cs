using Nuleep.Business.Interface;
using Nuleep.Data.Interface;
using Nuleep.Models;
using Nuleep.Models.Blogs;

namespace Nuleep.Business.Services
{
    public class JobService:IJobService
    {
        
        private readonly IJobRepository _jobRepository;

        public JobService(IJobRepository jobRepository)
        {
            _jobRepository = jobRepository;
        }
        public async Task<dynamic> CreateJob(int userId, Job job)
        {
            return await _jobRepository.CreateJob(userId, job);
        }
        public async Task<dynamic> GetJobById(int id)
        {
            return await _jobRepository.GetJobById(id);
        }
        public async Task<dynamic> GetAllRecruiterJobs()
        {
            return await _jobRepository.GetAllRecruiterJobs();
        }
    }
}
