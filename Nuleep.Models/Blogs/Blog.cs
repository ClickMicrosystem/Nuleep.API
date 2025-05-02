using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Nuleep.Models.Blogs
{
    public class Blog
    {
        [Key]
        public int Id { get; set; } // Assuming you want an Id field for the primary key

        [Required]
        public required string Title { get; set; }

        [Required]
        public required string Content { get; set; }

        public int EditedById { get; set; } // Foreign key for Profile

        public string? BlogImg_FileName { get; set; }
        public string? BlogImg_BlobName { get; set; }
        public string? BlogImg_FullUrl { get; set; }

        //[ForeignKey("EditedById")]
        //public virtual Profile EditedBy { get; set; }

        //public BlogImage? BlogImg { get; set; }

        public List<string> Likes { get; set; } = new List<string>();

        [Required]
        public string ContentMark { get; set; } = "both";

        public bool IsPublished { get; set; } = false;

        public DateTime? PublishDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class BlogImage
    {
        public string? BlogImg_FileName { get; set; }
        public string? BlogImg_BlobName { get; set; }
        public string? BlogImg_FullUrl { get; set; }
    }
}
