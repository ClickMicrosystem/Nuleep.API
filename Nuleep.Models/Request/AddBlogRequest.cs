using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Models.Request
{
    public class AddEditBlogRequest
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int EditedByProfileId { get; set; }
        public MediaImage BlogImg { get; set; }
        public List<int> Likes { get; set; } = new();
        public string ContentMark { get; set; } = "both";
        public bool IsPublished { get; set; } = false;
        public DateTime? PublishDate { get; set; }
    }
}
