using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nuleep.Business.Interface;
using Nuleep.Business.Services;
using Nuleep.Models.Blogs;
using Nuleep.Models.Request;

namespace Nuleep.API.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class BlogController : ControllerBase
    {
        private readonly IBlogService _blogService;
        private readonly AzureFileService _azurefileService;

        public BlogController(IBlogService blogService, AzureFileService azurefileService)
        {
            _blogService = blogService;
            _azurefileService = azurefileService;
        }

        [HttpPost("listAllBlog")]
        public async Task<IActionResult> GetAllBlogList(BlogRequest blogRequest)
        {
            var bloglist = await _blogService.GetAllBlogList(blogRequest);
            if (bloglist == null)
                return NotFound();
            return Ok(new { success = true, data = bloglist });
        }

        [HttpPost("addBlog")]
        public async Task<IActionResult> AddBlog([FromBody] AddEditBlogRequest request)
        {
            var blog = await _blogService.AddBlog(request);
            return Ok(new { success = true, data = blog });
        }

        //[HttpPost("imageUpload")]
        //public async Task<IActionResult> EditBlogImage([FromForm] IFormFile upload)
        //{
        //    var result = await _azurefileService.UploadAsync("blogs", upload);

        //    if (result.Success)
        //        return Ok(result);

        //    return StatusCode(500, new { success = false, message = "Upload failed" });
        //}

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteBlog([FromBody] DeleteBlogRequest request)
        {
            var result = await _blogService.DeleteBlog(request.BlogId);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost("fetchBlogDetails")]
        public async Task<IActionResult> GetBlogDetails([FromBody] GetBlogByIdRequest request)
        {
            var result = await _blogService.GetBlogDetails(request.Id);

            if (result.Id < 1)
                return NotFound(new { error = "Blog not found!" });

            return Ok(result);
        }

        [HttpPut("edit")]
        public async Task<IActionResult> EditBlog([FromBody] AddEditBlogRequest request)
        {
            var updatedBlog = await _blogService.EditBlog(request);

            if (updatedBlog.Id < 1l)
                return NotFound(new { error = "Blog not found!" });

            return Ok(new { success = true, data = updatedBlog });
        }

        [HttpPut("like/{id}")]
        public async Task<IActionResult> LikeBlog(string id, [FromBody] LikeBlogRequest request)
        {
            try
            {
                var result = await _blogService.ToggleLike(id, request.UserId);

                return Ok(new { message = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

    }
}
