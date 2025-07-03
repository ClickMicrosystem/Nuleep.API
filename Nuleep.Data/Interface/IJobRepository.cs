using Nuleep.Models;
using Nuleep.Models.Blogs;

namespace Nuleep.Data.Interface
{
    public interface IJobRepository
    {
        Task<ResponeModel> CreateJob(int userId, Job job);
        Task<dynamic> UpdateJob(int userId, Job job);
        Task<dynamic> GetJobById(int id);
        Task<ResponeModel> GetAllRecruiterJobs(int userId);
    }
}
