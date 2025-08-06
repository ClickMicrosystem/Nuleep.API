using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models.Response
{
    public class SignupResponse
    {
        public string Token { get; set; }
        public User Data { get; set; }
        public long ExpTime { get; set; }
    }

}
