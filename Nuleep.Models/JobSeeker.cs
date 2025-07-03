using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Nuleep.Models
{
    public class JobSeeker : Profile
    {
        public string Bio { get; set; }
        public MediaImage HeaderImage { get; set; }
        public CareerJourney? CareerJourney { get; set; }
        public MyStory? MyStory { get; set; }

        public bool? Remote { get; set; }

        [JsonPropertyName("skill")]
        public string Skills { get; set; }

        [JsonPropertyName("classe")]
        public string Classes { get; set; }

        public List<Job> SavedJobs { get; set; } = new();
        public List<Job> RecentlyViewJobs { get; set; } = new();

        public List<Award> Awards { get; set; } = new();

        public List<Course> RecentlyViwedCourses { get; set; } = new();
        public List<Course> SavedCourses { get; set; } = new();

        public List<Education> Education { get; set; } = new();
        public List<Experience> Experience { get; set; } = new();
        public List<Reference> References { get; set; } = new();
        public List<MediaImage> Interests { get; set; } = new();
        public List<MediaImage> Resume { get; set; } = new();


        public string CurrentCompany { get; set; }
        public string WebsiteUrl { get; set; }

        public string StreetAddress { get; set; }
        public string CountryRegion { get; set; }
        public string City { get; set; }
        public string StateProvince { get; set; }
        public string ZipPostal { get; set; }
        public string careerPath { get; set; }

        [JsonPropertyName("skills")]
        public List<string> SkillList { get; set; } = new();

        [JsonPropertyName("classes")]
        public List<string> ClassList { get; set; } = new();

        public List<int> SavedJobIds { get; set; } = new();

    }

}
