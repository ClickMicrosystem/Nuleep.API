using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models.Request
{
    public class ResetOwnershipRequest
    {
        public string Data { get; set; }
        public string OrgId { get; set; }
        public string Password { get; set; }
    }
}
