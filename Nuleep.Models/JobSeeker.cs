using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Nuleep.Models
{
    public class JobSeeker : Profile
    {
        public string Bio { get; set; }
        public ProfileImage HeaderImage { get; set; }

        public string CurrentCompany { get; set; }
        public bool Remote { get; set; }
        public string WebsiteUrl { get; set; }

        public string StreetAddress { get; set; }
        public string CountryRegion { get; set; }
        public string City { get; set; }
        public string StateProvince { get; set; }
        public string ZipPostal { get; set; }
        public string careerPath { get; set; }

        public CareerJourney CareerJourney { get; set; }
        public List<Award> Awards { get; set; } = new();
        public List<ProfileImage> ProjectImg { get; set; } = new();

        public List<Course> RecentlyViewedCourses { get; set; } = new();
        public List<Course> SavedCourses { get; set; } = new();

        public List<Education> Education { get; set; } = new();
        public List<Experience> Experience { get; set; } = new();
        public List<Reference> References { get; set; } = new();

        public MyStory? MyStory { get; set; }

        public List<string> Skills { get; set; } = new();
        public List<ProfileImage> Interests { get; set; } = new();
        public List<ProfileImage> Resume { get; set; } = new();
        public List<string> Classes { get; set; } = new();

        public List<int> SavedJobIds { get; set; } = new();

        public List<Job> SaveddJobs { get; set; } = new();
        public List<Job> RecentlyViewJobs { get; set; } = new();

    }

}
