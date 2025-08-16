using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models.Request
{
    public class JobSearchRequest
    {
        public string CompanyName { get; set; }
        public List<string> Benefits { get; set; }
        public string Culture { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public List<string> Compensation { get; set; }
        public List<string> Experience { get; set; }
        public List<string> JobType { get; set; }
        public int? MinSalary { get; set; }
        public int? MaxSalary { get; set; }
        public List<string> Skills { get; set; }
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
        public int PostingDateSort { get; set; } = -1; // -1 = DESC
    }

}
