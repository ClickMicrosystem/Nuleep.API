﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models.Request
{
    public class EventUnregisterRequest
    {
        public int EventId { get; set; }
        public int UserId { get; set; }
    }
}
