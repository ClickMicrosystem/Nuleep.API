using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models
{
    public class Subscription
    {
        public string Customer_Id { get; set; }
        public DateTime Period_Start { get; set; }
        public DateTime Period_End { get; set; }
        public string Status { get; set; }
        public string Plan_Id { get; set; }
        public DateTime Trial_Start { get; set; }
        public DateTime Trial_End { get; set; }
        public DateTime Billing_Cycle_Anchor { get; set; }
    }
}
