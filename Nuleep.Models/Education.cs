using System.Text.Json.Serialization;

namespace Nuleep.Models
{
    public class Education
    {
        [JsonPropertyName("_id")]
        public int? Id { get; set; }
        public int? ProfileId { get; set; }
        public string? SchoolOrOrganization { get; set; }
        public string? DegreeCertification { get; set; }
        public string? FieldOfStudy { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public bool Present { get; set; }
        public string? Description { get; set; }
    }

}
