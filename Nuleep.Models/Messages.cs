﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string? MessageContent { get; set; }
        public int? EditedBy { get; set; } // Foreign key to Profile
        public int? RoomId { get; set; } // Foreign key to Chatroom
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
