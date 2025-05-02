using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models
{
    public class ProfileRequest
    {
        public string? Role { get; set; } // jobSeeker or recruiter
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? JobTitle { get; set; }
        public string? Bio { get; set; }
        public string? About { get; set; }
        public string? StreetAddress { get; set; }
    }
}
