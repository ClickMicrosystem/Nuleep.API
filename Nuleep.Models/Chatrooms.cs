using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models
{
    public class Chatroom
    {
        public int? Id { get; set; }
        public string? Name { get; set; }
        public List<int> UserIds { get; set; } = new List<int>(); // List of user IDs
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
