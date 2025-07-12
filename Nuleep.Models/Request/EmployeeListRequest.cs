using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models.Request
{
    public class EmployeeListRequest
    {
        public int OrgId { get; set; }
        public bool Status { get; set; }
        public int Limit { get; set; } = 10;
        public int Page { get; set; } = 1;
    }

}
