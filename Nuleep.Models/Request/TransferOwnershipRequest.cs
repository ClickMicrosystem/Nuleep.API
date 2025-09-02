using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models.Request
{
    public class TransferOwnershipRequest
    {
        public string Email { get; set; }
        public string NewEmail { get; set; }
        public string OrgId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
