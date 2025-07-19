using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models.Request
{
    public class EventListRequest
    {
        public int Page { get; set; } = 1;
        public int Limit { get; set; } = 10;
        public bool? IsDelete { get; set; }
        public string? Type { get; set; }
    }
}
