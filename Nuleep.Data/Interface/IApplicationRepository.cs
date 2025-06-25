using Nuleep.Models;

namespace Nuleep.Data.Interface
{
    public interface IApplicationRepository
    {
        Task<dynamic> GetAllRecruiterApplications(string username);
        Task<dynamic> GetAllJobSeekerApplications(string username);
        Task<dynamic> GetApplicationsByJob(int jobId);
        Task<dynamic> CreateApplication(int jobId, Application application);

    }
}
