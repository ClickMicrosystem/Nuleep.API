using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models
{
    public class Profile
    {
        public int Id { get; set; } // Assume Identity Primary Key

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string? Email { get; set; }
        public string? JobTitle { get; set; }
        public bool IsDelete { get; set; } = false;
        public int? UserId { get; set; } // shall be removed

        public User? User { get; set; }
        //public User? UserRefId { get; set; }
        public string? Phone { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? ChatRoomsId { get; set; } // shall be removed
        public int? ProfileImgId { get; set; } // shall be removed
        public List<Chatroom> ChatRooms { get; set; } = new();
        public List<ProfileImage> ProfileImg { get; set; } = new();
    }

}
