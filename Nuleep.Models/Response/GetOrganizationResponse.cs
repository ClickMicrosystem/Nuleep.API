using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models.Response
{
    public class GetOrganizationResponse
    {
        public bool Success { get; set; }
        public int Count { get; set; }
        public object Data { get; set; }
    }

}
