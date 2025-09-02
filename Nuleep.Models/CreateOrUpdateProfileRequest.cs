using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models
{
    public class CreateOrUpdateProfileRequest
    {
        // Common Fields
        public int? UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? JobTitle { get; set; }
        public bool? IsDelete { get; set; }
        public string? Phone { get; set; }
        public string? Role { get; set; }
        public List<ChatRoom>? ChatRooms { get; set; }
        public List<MediaImage>? ProfileImg { get; set; }

        public List<Education>? Education { get; set; }
        public List<Award>? Awards { get; set; }


        // JobSeeker-specific
        public string? Bio { get; set; }
        public FileUploadModel? HeaderImage { get; set; }
        public string? CurrentCompany { get; set; }
        public bool? Remote { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? CareerPath { get; set; }
        public string? StreetAddress { get; set; }
        public string? CountryRegion { get; set; }
        public string? City { get; set; }
        public string? StateProvince { get; set; }
        public string? ZipPostal { get; set; }
        public CareerJourney? CareerJourney { get; set; }
        public List<MediaImage>? ProjectImg { get; set; }

        //public List<CourseModel>? RecentlyViwedCourses { get; set; } // discussion required and shouldnt be in create profile request

        //public List<CourseModel>? SavedCourses { get; set; } // discussion required and shouldnt be in create profile request

        public List<Experience>? Experience { get; set; }
        public List<Reference>? References { get; set; }
        public MyStory? MyStory { get; set; }
        public List<string>? Skills { get; set; }
        public List<MediaImage>? Interests { get; set; }
        public List<MediaImage>? Resume { get; set; }
        public List<string>? Classes { get; set; }

        //public List<Job>? SavedJobs { get; set; } // discussion required and shouldnt be in create profile request

        // public List<Job>? RecentlyViewJobs { get; set; } // discussion required and shouldnt be in create profile request

        // Recruiter-specific
        public string? About { get; set; }
        public string? Title { get; set; }

        //public Organization? Organization { get; set; } // discussion required and shouldnt be in create profile request

        // public List<User>? SavedCandidates { get; set; } // discussion required and shouldnt be in create profile request
        public string? OrganizationRole { get; set; }
        public bool? OrganizationApproved { get; set; }
    }

    public class FileUploadModel
    {
        public string? FileName { get; set; }
        public string? BlobName { get; set; }
        public string? FullUrl { get; set; }
    }

    public class AwardModel
    {
        public string CompanyName { get; set; }
        public string AwardName { get; set; }
        public DateTime? Date { get; set; }
        public string Description { get; set; }
    }

    public class CourseModel
    {
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public string Url { get; set; }
        public double Rating { get; set; }
        public string Duration { get; set; }
        public string Provider { get; set; }
    }

    public class EducationModel
    {
        public string? SchoolOrOrganization { get; set; }
        public string? DegreeCertification { get; set; }
        public string? FieldOfStudy { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public bool Present { get; set; }
        public string Description { get; set; }
    }

    public class ExperienceModel
    {
        public string Title { get; set; }
        public string Company { get; set; }
        public List<string> Description { get; set; }
        public string Location { get; set; }
        public string DescriptionC { get; set; }
        public List<string> Impact { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public bool Current { get; set; }
    }

    public class ReferenceModel
    {
        public string Name { get; set; }
        public string Company { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }

    public class CareerJourneyModel
    {
        public string? NextRole { get; set; }
        public string? Description { get; set; }
        public List<string>? Experience { get; set; }
        public List<string>? Training { get; set; }
    }

    public class MyStoryModel
    {
        public string Header { get; set; }
        public string Summary { get; set; }
        public List<MyStoryActivityModel> Activities { get; set; }
    }

    public class MyStoryActivityModel
    {
        public string Title { get; set; }
        public MyStoryImageModel Image { get; set; }
        public List<string> Skills { get; set; }
    }

    public class MyStoryImageModel
    {
        public string BlobName { get; set; }
        public string FullUrl { get; set; }
    }

}
