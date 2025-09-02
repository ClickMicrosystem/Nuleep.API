using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models.Request
{
    public class AddMessageRequest
    {
        public string Message { get; set; } = string.Empty;
        public int UserId { get; set; }
        public int RoomId { get; set; }
    }
}
