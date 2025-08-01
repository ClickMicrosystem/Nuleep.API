using Nuleep.Models;
using Nuleep.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Business.Interface
{
    public interface IApplicationService
    {
        Task<dynamic> GetAllRecruiterApplications(string username);
        Task<dynamic> GetAllJobSeekerApplications(string username);
        Task<dynamic> GetApplicationsByJob(int jobId, int userId);
        Task<dynamic> CreateApplication(int jobId, Application application, int userId);
        Task<ApplicationDetail> GetApplicationById(int applicationId, int userId);
        Task<ApplicationDetail> UpdateApplication(int applicationId, int userId, Application request);
        Task<bool> DeleteApplication(int applicationId, int userId);

    }
}
