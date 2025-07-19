using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models.Request
{
    public class EventCheckoutRequest
    {
        public string Email { get; set; }
        public string Description { get; set; }
        public string EventName { get; set; }
        public string ImageUrl { get; set; }
        public int UserId { get; set; }
        public int EventId { get; set; }
        public decimal Amount { get; set; }
    }
}
