using Nuleep.Models;
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
        Task<dynamic> GetApplicationsByJob(int jobId);
        Task<dynamic> CreateApplication(int jobId, Application application);
    }
}
