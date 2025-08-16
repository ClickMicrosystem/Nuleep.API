using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models.Request
{
    public class JobsTransferRequest
    {
        public int RecId { get; set; }
        public int NewRecId { get; set; }
        public int UId { get; set; }
    }

}
