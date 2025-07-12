using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models.Request
{
    public class EditCompanyRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string About { get; set; }
        public string Culture { get; set; }
        public string Mission { get; set; }

        public List<string> Benefits { get; set; } = new();
        public List<string> Perks { get; set; } = new();

        public string ZipPostal { get; set; }
        public string City { get; set; }

        public bool ShowRequired { get; set; }

        public int OrgId { get; set; } // Include this if you're identifying the organization to edit
    }

}
