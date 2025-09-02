using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models
{
    public class ChatRoom
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public List<int> UserIds { get; set; } = new List<int>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
