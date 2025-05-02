using System.Text.Json.Serialization;

namespace Nuleep.Models
{
    public class Award
    {
        [JsonPropertyName("_id")]
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string AwardName { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
    }

}
