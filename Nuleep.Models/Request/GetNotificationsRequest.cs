using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models.Request
{
    public class GetNotificationsRequest
    {
        public string UserId { get; set; }
        public List<string> NotificationType { get; set; } = new();
    }
}
