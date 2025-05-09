using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models
{
    public class Job
    {
        public int Id { get; set; }
        public string PositionTitle { get; set; } = string.Empty;
        public string? Experience { get; set; }
        public required string Location { get; set; };
        public string? Description { get; set; }
        public string? Department { get; set; }
        public List<string> Requirements { get; set; } = new List<string>();
        public required List<string> SkillKeywords { get; set; }
        public string? JobType { get; set; }
        public string? SalaryType { get; set; }
        public int? Salary { get; set; }
        public string? Remote { get; set; }
        public string? RequisitionNumber { get; set; }
        public DateTime? PostingDate { get; set; }
        public DateTime? ClosingDate { get; set; }

        public List<Application>? Application { get; set; }
        public string? CompanyContact { get; set; }
        public string? CompanyEmail { get; set; }

        public Organization? Organization { get; set; }
        public Recruiter? Recruiter { get; set; }
        public string? Program { get; set; }
        public string? ExperienceLevel { get; set; }
        public string? NuleepID { get; set; }
    }
}
