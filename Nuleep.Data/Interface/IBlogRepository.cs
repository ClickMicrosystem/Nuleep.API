using Nuleep.Models;
using Nuleep.Models.Blogs;
using Nuleep.Models.Request;
using Nuleep.Models.Response;

namespace Nuleep.Data
{
    public interface IBlogRepository
    {
        Task<dynamic> GetAllBlogList(BlogRequest blogRequest);
        Task<BlogResponse> AddBlog(AddEditBlogRequest request);
        Task<BlogResponse> GetBlogById(int id);
        Task DeleteBlog(int id);
        Task<BlogResponse> EditBlog(AddEditBlogRequest request);
        Task UpdateLikes(string blogId, List<string> likes);
    }
}
