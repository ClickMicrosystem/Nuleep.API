using Nuleep.Models;
using Nuleep.Models.Blogs;
using Nuleep.Models.Request;
using Nuleep.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Business.Interface
{
    public interface IBlogService
    {
        Task<dynamic> GetAllBlogList(BlogRequest blogRequest);
        Task<BlogResponse> AddBlog(AddEditBlogRequest request);
        Task<DeleteBlogResponse> DeleteBlog(int blogId);
        Task<BlogResponse> GetBlogDetails(int blogId);
        Task<BlogResponse> EditBlog(AddEditBlogRequest request);
        Task<string> ToggleLike(string blogId, string userId);

    }
}
