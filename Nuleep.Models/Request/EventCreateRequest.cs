using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models.Request
{
    public class EventEditCreateRequest
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Summary { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Price { get; set; }
        public string? TeleconferenceLink { get; set; }
        public string? EventPhysicalAddress { get; set; }
        public string? Location { get; set; }
        public List<string>? EventTags { get; set; }
        public MediaImage? MediaImage { get; set; }
        public bool IsDelete { get; set; } = false;
        public bool IsNotify { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

}
