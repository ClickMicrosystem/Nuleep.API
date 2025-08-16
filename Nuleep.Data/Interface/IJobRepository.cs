using Nuleep.Models;
using Nuleep.Models.Blogs;
using Nuleep.Models.Request;

namespace Nuleep.Data.Interface
{
    public interface IJobRepository
    {
        Task<ResponeModel> CreateJob(int userId, Job job);
        Task<dynamic> UpdateJob(int userId, Job job);
        Task<dynamic> GetJobById(int id);
        Task<ResponeModel> GetAllRecruiterJobs(int userId);
        Task<(List<Job>, int)> GetAllJobs(JobSearchRequest request);
        Task<IEnumerable<int>> GetJobIdsByRecruiter(int recId);
        Task UpdateJobsRecruiter(IEnumerable<int> jobIds, int newRecId);
        Task MarkUserDeleted(int userId);
        Task MarkProfileDeleted(int profileId);
    }
}
