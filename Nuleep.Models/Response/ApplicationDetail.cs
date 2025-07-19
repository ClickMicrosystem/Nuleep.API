using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models.Response
{
    public class ApplicationDetail
    {
        public int Id { get; set; }
        public Job Job { get; set; }
        public Profile Profile { get; set; }
    }

}
