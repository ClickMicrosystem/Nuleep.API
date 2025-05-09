using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Nuleep.Models
{
    public class Recruiter : Profile
    {
        [JsonPropertyName("_id")]
        public int Id { get; set; }

        public string About { get; set; }

        public string Bio { get; set; }

        public string StreetAddress { get; set; }

        public string Title { get; set; }

        public Organization Organization { get; set; }

        public string OrganizationRole { get; set; } = "unapproved";

        public bool OrganizationApproved { get; set; } = false;

        public List<User>? savedCandidates { get; set; }

        public DateTime CreatedAt { get; set; }

        public List<Education> Education { get; set; }

        public List<Award> Awards { get; set; }

    }
    
}
