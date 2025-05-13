using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models
{
    public class ProfileResponse
    {
        // Shared
        public Guid Id { get; set; }
        public string? Type { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FullName => $"{FirstName} {LastName}";
        public string? Email { get; set; }
        public string? JobTitle { get; set; }
        public string? Phone { get; set; }
        public bool IsDelete { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid UserRef { get; set; }
        public List<ChatRoomDto> ChatRooms { get; set; } = new();
        public List<MediaDto> ProfileImg { get; set; } = new();

        // JobSeeker
        public string? Bio { get; set; }
        public MediaDto? HeaderImage { get; set; }
        public string? CurrentCompany { get; set; }
        public bool? Remote { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? CareerPath { get; set; }
        public string? StreetAddress { get; set; }
        public string? CountryRegion { get; set; }
        public string? City { get; set; }
        public string? StateProvince { get; set; }
        public string? ZipPostal { get; set; }

        public CareerJourneyDto? CareerJourney { get; set; }
        public List<AwardDto> Awards { get; set; } = new();
        public List<MediaDto> ProjectImg { get; set; } = new();
        public List<CourseDto> RecentlyViewedCourses { get; set; } = new();
        public List<CourseDto> SavedCourses { get; set; } = new();
        public List<EducationDto> Education { get; set; } = new();
        public List<ExperienceDto> Experience { get; set; } = new();
        public List<ReferenceDto> References { get; set; } = new();
        public MyStoryDto? MyStory { get; set; }
        public List<string> Skills { get; set; } = new();
        public List<MediaDto> Interests { get; set; } = new();
        public List<MediaDto> Resume { get; set; } = new();
        public List<string> Classes { get; set; } = new();
        public List<Guid> SavedJobs { get; set; } = new();
        public List<Guid> RecentlyViewedJobs { get; set; } = new();

        // Recruiter
        public string? About { get; set; }
        public string? Title { get; set; }
        public Guid? Organization { get; set; }
        public List<Guid> SavedCandidates { get; set; } = new();
        public string? OrganizationRole { get; set; }
        public bool? OrganizationApproved { get; set; }
        public List<EducationDto> Nnn1 { get; set; } = new();  // Optional recruiter education
    }


    public class MediaDto
    {
        public string? FileName { get; set; }
        public string? BlobName { get; set; }
        public string? FullUrl { get; set; }
    }

    public class ChatRoomDto
    {
        public Guid RoomId { get; set; }
        public string? RoomName { get; set; }
        public string? Image { get; set; }
    }

    public class AwardDto
    {
        public string? CompanyName { get; set; }
        public string? AwardName { get; set; }
        public DateTime? Date { get; set; }
        public string? Description { get; set; }
    }

    public class CourseDto
    {
        public int CourseId { get; set; }
        public string? Title { get; set; }
        public string? Image { get; set; }
        public string? Url { get; set; }
        public double? Rating { get; set; }
        public string? Duration { get; set; }
        public string? Provider { get; set; }
    }

    public class EducationDto
    {
        public string? SchoolOrOrganization { get; set; }
        public string? DegreeCertification { get; set; }
        public string? FieldOfStudy { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public bool? Present { get; set; }
        public string? Description { get; set; }
    }

    public class ExperienceDto
    {
        public string? Title { get; set; }
        public string? Company { get; set; }
        public List<string> Description { get; set; } = new();
        public string? Location { get; set; }
        public string? DescriptionC { get; set; }
        public List<string> Impact { get; set; } = new();
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public bool? Current { get; set; }
    }

    public class ReferenceDto
    {
        public string? Name { get; set; }
        public string? Company { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
    }

    public class CareerJourneyDto
    {
        public string? NextRole { get; set; }
        public string? Description { get; set; }
        public List<string> Experience { get; set; } = new();
        public List<string> Training { get; set; } = new();
    }

    public class MyStoryDto
    {
        public string? Header { get; set; }
        public string? Summary { get; set; }
        public List<MyStoryActivityDto> Activities { get; set; } = new();
    }

    public class MyStoryActivityDto
    {
        public string? Title { get; set; }
        public MediaDto? Image { get; set; }
        public List<string> Skills { get; set; } = new();
    }

}
