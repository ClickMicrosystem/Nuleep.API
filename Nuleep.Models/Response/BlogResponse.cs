using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models.Response
{
    public class BlogResponse
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public dynamic EditedByProfile { get; set; }
        public MediaImage BlogImg { get; set; }

        public List<string> Likes { get; set; } = new();
        public string ContentMark { get; set; }
        public bool IsPublished { get; set; }
        public DateTime? PublishDate { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
