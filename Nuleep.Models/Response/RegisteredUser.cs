using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models.Response
{
    public class RegisteredUser
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string JobTitle { get; set; }
        public string StreetAddress { get; set; }
    }

}
