using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models
{
    public class Event
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Summary { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? Price { get; set; }
        public string? TeleconferenceLink { get; set; }
        public string? EventPhysicalAddress { get; set; }
        public string? Location { get; set; }
        public string EventTags { get; set; } = "[]"; // JSON string representation
        public string? EventImgFileName { get; set; }
        public string? EventImgBlobName { get; set; }
        public string? EventImgFullUrl { get; set; }
        public bool IsDelete { get; set; } = false;
        public bool IsNotify { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public List<EventRegistration> Registrations { get; set; } = new List<EventRegistration>();
    }

    public class EventRegistration
    {
        public int EventId { get; set; }
        public int UserId { get; set; }
        public string? PaymentId { get; set; }
        public bool IsRefunded { get; set; } = false;
    }
}
