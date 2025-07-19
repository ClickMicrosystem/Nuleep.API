using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models.Response
{
    public class PagedResult<T>
    {
        public List<T> Data { get; set; }
        public int Total { get; set; }
    }
}
