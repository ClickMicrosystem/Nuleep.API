using System.Text.Json.Serialization;

namespace Nuleep.Models
{
    public class User
    {
        public string? Username { get; set; } // didnt had in UserSchema

        [JsonPropertyName("_id")]
        public int Id { get; set; }
        public string Email { get; set; }
        public bool ValidateEmail { get; set; } = false;
        public bool IsProfile { get; set; } = false;
        public string GoogleId { get; set; }
        public string Password { get; set; }
        public string ResetPasswordToken { get; set; }
        public DateTime? ResetPasswordExpire { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsDelete { get; set; } = false;

        public Subscription? subscription { get; set; }
    }
}
