using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models.Response
{
    public class GoogleLoginResponse
    {
        public string Token { get; set; }
        public bool HasProfile { get; set; }
        public string Email { get; set; }
    }

}
