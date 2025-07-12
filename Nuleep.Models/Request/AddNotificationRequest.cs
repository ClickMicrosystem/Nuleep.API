using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models.Request
{
    public class AddNotificationRequest
    {
        public string Title { get; set; }
        public string UserId { get; set; }
        public string RoomId { get; set; }
    }
}
