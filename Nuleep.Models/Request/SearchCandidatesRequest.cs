using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models.Request
{
    public class SearchCandidatesRequest
    {
        public string Name { get; set; }
        public int? Limit { get; set; }
        public int? Page { get; set; }
    }
}
