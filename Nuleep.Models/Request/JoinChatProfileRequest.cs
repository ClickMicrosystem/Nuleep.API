using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models.Request
{
    public class JoinChatProfileRequest
    {
        public ProfileReq JobSeek { get; set; }
        public ProfileReq JobReq { get; set; }
    }

    public class ProfileReq
    {
        public int Id { get; set; }
        public string Email { get; set; }
    }
}
