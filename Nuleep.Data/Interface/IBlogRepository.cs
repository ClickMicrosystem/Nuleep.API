using Nuleep.Models;
using Nuleep.Models.Blogs;

namespace Nuleep.Data
{
    public interface IBlogRepository
    {
        Task<dynamic> GetAllBlogList(BlogRequest blogRequest);
    }
}
