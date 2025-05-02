using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public bool IsRead { get; set; } = false;
        public string? Title { get; set; }
        public string? RoomId { get; set; }
        public string? NotificationType { get; set; }
        public string? UserId { get; set; }
        public string? EventId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
