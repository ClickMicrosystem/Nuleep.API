using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models
{
    public class Application
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Submitted";
        public int JobId { get; set; }
        public Job Job { get; set; }
        public int ProfileId { get; set; }
        public Profile Profile { get; set; }

        public bool IsSaved { get; set; } = false;
        public bool IsArchived { get; set; } = false;
        public bool IsRemoved { get; set; } = false;

        public string CoverLetter { get; set; }

        public List<ApplicationSkill> Skills { get; set; }
        public List<ApplicationTimelineEvent> Timeline { get; set; }
    }

    public class ApplicationSkill
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public string Skill { get; set; }
    }

    public class ApplicationTimelineEvent
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public string Event { get; set; }
    }
}
