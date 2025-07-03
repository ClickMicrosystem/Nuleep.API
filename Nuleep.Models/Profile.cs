using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Nuleep.Models
{
    public class Profile
    {
        [JsonPropertyName("_id")]
        public int Id { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string? Email { get; set; }
        public string? JobTitle { get; set; }
        public string? Type { get; set; }
        public bool? IsDelete { get; set; } = false;
        public int? UserId { get; set; }

        [JsonPropertyName("userRef")]
        public User? User { get; set; }
        public string? Phone { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? ChatRoomsId { get; set; }
        public int? ProfileImgId { get; set; }
        public List<Chatroom> ChatRooms { get; set; } = new();

        [JsonPropertyName("profileImg")]
        public List<MediaImage> ProfileImage { get; set; } = new();
        public List<MediaImage> ProjectImg { get; set; } = new();
    }

}
