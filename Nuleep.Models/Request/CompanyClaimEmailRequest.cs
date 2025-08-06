using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models.Request
{
    public class CompanyClaimEmailRequest
    {
        public int OrgId { get; set; }
        public string FName { get; set; }
        public string LName { get; set; }
        public string ReqEmail { get; set; }
        public string CompName { get; set; }
    }
}
