using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models.Request
{
    public class ApproveJoinRequest
    {
        public int ProfileId { get; set; }
        public string Role { get; set; } = string.Empty;
    }

}
