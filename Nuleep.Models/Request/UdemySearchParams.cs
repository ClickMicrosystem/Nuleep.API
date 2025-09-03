using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models.Request
{
    public class UdemySearchParams
    {
        public int Page { get; set; } = 1;
        public int Size { get; set; } = 10;
        public string Search { get; set; } = "";
    }
}
