using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models.Response
{
    public class AdminSigninResponse
    {
        public string Email { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }
    }

}
