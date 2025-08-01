using Nuleep.Models;
using Nuleep.Models.Response;

namespace Nuleep.Data.Interface
{
    public interface IApplicationRepository
    {
        Task<dynamic> GetAllRecruiterApplications(string username);
        Task<dynamic> GetAllJobSeekerApplications(string username);
        Task<dynamic> GetApplicationsByJob(int jobId, int userId);
        Task<dynamic> CreateApplication(int jobId, Application application, int userId);
        Task<ApplicationDetail> GetApplicationById(int applicationId);
        Task<ApplicationDetail?> UpdateApplication(int applicationId, Application request);
        Task<bool> DeleteApplication(int applicationId);

    }
}
