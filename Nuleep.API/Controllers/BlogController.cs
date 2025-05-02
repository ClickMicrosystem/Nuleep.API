using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nuleep.Business.Interface;
using Nuleep.Models.Blogs;

namespace Nuleep.API.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BlogController : ControllerBase
    {
        private readonly IBlogService _blogService;

        public BlogController(IBlogService blogService)
        {
            _blogService = blogService;
        }

        [HttpPost("listAllBlog")]
        public async Task<IActionResult> GetAllBlogList(BlogRequest blogRequest)
        {
            var bloglist = await _blogService.GetAllBlogList(blogRequest);
            if (bloglist == null)
                return NotFound();
            return Ok(new { success = true, data = bloglist });
        }
    }
}
