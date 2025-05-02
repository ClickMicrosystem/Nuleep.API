using Nuleep.Business.Interface;
using Nuleep.Data;
using Nuleep.Data.Interface;
using Nuleep.Models;
using Nuleep.Models.Blogs;

namespace Nuleep.Business.Services
{
    public class BlogService:IBlogService
    {
        
        private readonly IBlogRepository _blogRepository;

        public BlogService(IBlogRepository blogRepository)
        {
            _blogRepository = blogRepository;
        }
        public async Task<dynamic> GetAllBlogList(BlogRequest blogRequest)
        {
            return await _blogRepository.GetAllBlogList(blogRequest);
        }
    }
}
