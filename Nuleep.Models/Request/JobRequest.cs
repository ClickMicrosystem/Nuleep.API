using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models.Request
{
    public class JobRequest
    {
        public string PositionTitle { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;

        public List<string> Requirements { get; set; } = new();
        public List<string> SkillKeywords { get; set; } = new();

        public string JobType { get; set; } = string.Empty;
        public string SalaryType { get; set; } = string.Empty;
        public string Salary { get; set; } = string.Empty;
        public string Remote { get; set; } = string.Empty;

        public string RequisitionNumber { get; set; } = string.Empty;

        public DateTime? PostingDate { get; set; }
        public DateTime? ClosingDate { get; set; }

        public string CompanyContact { get; set; } = string.Empty;
        public string CompanyEmail { get; set; } = string.Empty;

        public bool ShowRequired { get; set; }

        public int OrgId { get; set; } // From props.match.params.id
    }


}
