using Nuleep.Business.Interface;
using Nuleep.Data;
using Nuleep.Data.Interface;
using Nuleep.Models;
using Nuleep.Models.Blogs;
using Nuleep.Models.Request;
using Nuleep.Models.Response;

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
        public async Task<BlogResponse> AddBlog(AddEditBlogRequest request)
        {
            return await _blogRepository.AddBlog(request);
        }

        public async Task<DeleteBlogResponse> DeleteBlog(int blogId)
        {
            var existing = await _blogRepository.GetBlogById(blogId);
            if (existing == null)
            {
                return new DeleteBlogResponse
                {
                    Success = false,
                    Message = "Blog not found!"
                };
            }

            await _blogRepository.DeleteBlog(blogId);

            return new DeleteBlogResponse
            {
                Success = true,
                Message = "Blog deleted successfully"
            };
        }

        public async Task<BlogResponse> GetBlogDetails(int blogId)
        {
            return await _blogRepository.GetBlogById(blogId);
        }

        public async Task<BlogResponse> EditBlog(AddEditBlogRequest request)
        {
            return await _blogRepository.EditBlog(request);
        }
        public async Task<string> ToggleLike(string blogId, string userId)
        {
            var blog = await _blogRepository.GetBlogById(int.Parse(blogId));
            if (blog == null)
                throw new Exception("Blog not found");

            if (!blog.Likes.Contains(userId))
            {
                blog.Likes.Add(userId);
                await _blogRepository.UpdateLikes(blogId, blog.Likes);
                return "The blog has been liked";
            }
            else
            {
                blog.Likes.Remove(userId);
                await _blogRepository.UpdateLikes(blogId, blog.Likes);
                return "The blog has been disliked";
            }
        }


    }
}
