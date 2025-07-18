﻿using Nuleep.Models;
using Nuleep.Models.Blogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Business.Interface
{
    public interface IJobService
    {
        Task<ResponeModel> CreateJob(int userId, Job job);
        Task<dynamic> UpdateJob(int userId, Job job);
        Task<dynamic> GetJobById(int id);
        Task<ResponeModel> GetAllRecruiterJobs(int userId);
    }
}
