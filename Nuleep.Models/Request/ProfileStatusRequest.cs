using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models.Request
{
    public class ProfileStatusRequest
    {
        public int Id { get; set; }
        public ProfileStatusData Data { get; set; }
    }

    public class ProfileStatusData
    {
        public bool IsProfile { get; set; }
        public bool ValidateEmail { get; set; }
    }

}
