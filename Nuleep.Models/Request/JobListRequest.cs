using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models.Request
{
    public class JobListRequest
    {
        public int OrgId { get; set; }
        public int Limit { get; set; } = 10;
        public int Page { get; set; } = 1;
    }

}
