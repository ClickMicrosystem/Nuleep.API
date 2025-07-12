using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models.Request
{
    public class ReadNotificationRequest
    {
        public int? Id { get; set; }
        public string? UserId { get; set; }
        public List<string>? NotificationType { get; set; }
        public string? RoomId { get; set; }
    }
}
