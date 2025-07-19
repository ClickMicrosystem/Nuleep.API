using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models.Request
{
    public class JobSeekerEventFilterRequest
    {
        public string? Title { get; set; }
        public string? PostDate { get; set; }
        public List<string>? EventTags { get; set; }
        public int Limit { get; set; } = 100;
        public int Page { get; set; } = 1;
    }

}
