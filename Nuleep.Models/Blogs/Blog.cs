using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Nuleep.Models.Blogs
{
    public class Blog
    {
        [Key]
        public int Id { get; set; }

        public string? Title { get; set; }

        [Required]
        public required string Content { get; set; }

        public int? EditedById { get; set; } // Foreign key for Profile

        public string? BlogImg_FileName { get; set; }
        public string? BlogImg_BlobName { get; set; }
        public string? BlogImg_FullUrl { get; set; }

        public string Likes { get; set; }

        [Required]
        public string ContentMark { get; set; } = "both";

        public bool IsPublished { get; set; } = false;

        public DateTime? PublishDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
