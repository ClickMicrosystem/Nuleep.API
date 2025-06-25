using Nuleep.Models;

namespace Nuleep.Models
{
    public class CreateProfileRequest
    {
        // Common Profile Fields
        public int UserId { get; set; }
        public string? Role { get; set; } // "jobSeeker" or "recruiter"
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? JobTitle { get; set; }
        public string? Phone { get; set; }

        public bool isDelete { get; set; } = false;

        // Shared
        public List<Chatroom>? ChatRooms { get; set; }
        public List<MediaImage>? ProfileImg { get; set; }

        // ──────────────── JobSeeker-specific fields ────────────────
        public string? Bio { get; set; }
        public string? CurrentCompany { get; set; }
        public bool? Remote { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? CareerPath { get; set; }

        public string? StreetAddress { get; set; }
        public CareerJourney? CareerJourney { get; set; }

        public List<Award>? Awards { get; set; }
        public List<MediaImage>? ProjectImg { get; set; }

        public List<Course>? RecentlyViewedCourses { get; set; }
        public List<Course>? SavedCourses { get; set; }

        public List<Education>? Education { get; set; }
        public List<Experience>? Experience { get; set; }
        public List<Reference>? References { get; set; }

        public MyStory? MyStory { get; set; }
        public List<string>? Skills { get; set; }
        public List<MediaImage>? Interests { get; set; }
        public List<MediaImage>? Resume { get; set; }

        public List<string>? Classes { get; set; }
        public List<int>? SavedJobs { get; set; }
        public List<int>? RecentlyViewedJobs { get; set; }

        // ──────────────── Recruiter-specific fields ────────────────
        public string? About { get; set; }
        public string? Title { get; set; }
        public int? OrganizationId { get; set; }
        public List<int>? SavedCandidates { get; set; }

        public string? OrganizationRole { get; set; } // "admin", "user", "unapproved"
        public bool? OrganizationApproved { get; set; }
    }


    
}
