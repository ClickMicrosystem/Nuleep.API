using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models.Request
{
    public class DeleteCompanyLogoRequest
    {
        public int OrgId { get; set; }
        public string FileName { get; set; }
    }
}
