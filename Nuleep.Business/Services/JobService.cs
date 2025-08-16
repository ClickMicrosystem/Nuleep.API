using Nuleep.Business.Interface;
using Nuleep.Data.Interface;
using Nuleep.Models;
using Nuleep.Models.Blogs;
using Nuleep.Models.Request;

namespace Nuleep.Business.Services
{
    public class JobService:IJobService
    {
        
        private readonly IJobRepository _jobRepository;

        public JobService(IJobRepository jobRepository)
        {
            _jobRepository = jobRepository;
        }
        public async Task<ResponeModel> CreateJob(int userId, Job job)
        {
            return await _jobRepository.CreateJob(userId, job);
        }
        public async Task<dynamic> UpdateJob(int userId, Job job)
        {
            return await _jobRepository.UpdateJob(userId, job);
        }
        public async Task<dynamic> GetJobById(int id)
        {
            return await _jobRepository.GetJobById(id);
        }
        public async Task<ResponeModel> GetAllRecruiterJobs(int userId)
        {
            return await _jobRepository.GetAllRecruiterJobs(userId);
        }

        public async Task<(List<Job>, int)> GetAllJobs(JobSearchRequest request)
        {
            return await _jobRepository.GetAllJobs(request);            
        }

        public async Task TransferJobs(JobsTransferRequest request)
        {
            var jobIds = await _jobRepository.GetJobIdsByRecruiter(request.RecId);

            if (!jobIds.Any())
                throw new KeyNotFoundException("No jobs found for this recruiter.");

            await _jobRepository.UpdateJobsRecruiter(jobIds, request.NewRecId);
            await _jobRepository.MarkUserDeleted(request.UId);
            await _jobRepository.MarkProfileDeleted(request.RecId);
        }
    }
}
