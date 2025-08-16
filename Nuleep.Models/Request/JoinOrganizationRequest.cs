using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models.Request
{
    public class JoinOrganizationRequest
    {
        public string OrgCode { get; set; }
        public int OrgId { get; set; }
        public int UserId { get; set; }
    }

}
