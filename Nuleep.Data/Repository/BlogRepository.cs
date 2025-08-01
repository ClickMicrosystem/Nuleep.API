using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Nuleep.Models;
using System.Data;
using Dapper;
using Nuleep.Models.Blogs;
using Azure.Core;
using Nuleep.Data.Interface;
using Nuleep.Models.Request;
using Nuleep.Models.Response;
using System.Data.Common;
using Newtonsoft.Json;

namespace Nuleep.Data.Repository
{
    public class BlogRepository : IBlogRepository
    {
        private readonly IDbConnection _db;
        private readonly IProfileRepository _profileRepository;

        public BlogRepository(IConfiguration config, IProfileRepository profileRepository)
        {
            _db = new SqlConnection(config.GetConnectionString("DefaultConnection"));
            _profileRepository = profileRepository;
        }

        public async Task<dynamic> GetAllBlogList(BlogRequest blogRequest)
        {

            //var sql = "SELECT * FROM Users WHERE Username = @Username AND Password = @Password";  
            //return await _db.QueryFirstOrDefaultAsync<Blog>(sql, new { Username = username, Password = password });

            int limit = blogRequest.Limit > 0 ? blogRequest.Limit : 10;         
            int page = blogRequest.Page > 0 ? blogRequest.Page - 1 : 0;

            _db.Open();

            string sqlQuery;
            if (blogRequest.Type == "admin")
            {
                sqlQuery = "SELECT * FROM Blogs ORDER BY Id DESC OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";
            }
            else
            {
                sqlQuery = @"
                SELECT * FROM Blogs 
                WHERE (ContentMark = @Type OR ContentMark = 'both') AND IsPublished = 1 
                ORDER BY Id DESC 
                OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY";
            }

            var list = await _db.QueryAsync<Blog>(sqlQuery, new { Offset = limit * page, Limit = limit, blogRequest.Type });

            string countQuery = @"
            SELECT COUNT(*) FROM Blogs 
            WHERE (ContentMark = @Type OR ContentMark = 'both') AND IsPublished = 1";

            var total = await _db.ExecuteScalarAsync<int>(countQuery, new { blogRequest.Type });

            return  new { data = list, total };            
        }

        public async Task<BlogResponse> AddBlog(AddEditBlogRequest request)
        {
            BlogResponse blogResponse = new BlogResponse();
            var sql = @"
                        INSERT INTO Blogs (Title, Content, EditedById, ContentMark, IsPublished, PublishDate, CreatedAt)
                        OUTPUT INSERTED.*
                        VALUES (@Title, @Content, @EditedById, @ContentMark, @IsPublished, @PublishDate, @CreatedAt)";



            var blog = await _db.QuerySingleAsync<Blog>(sql, new
            {
                Title = request.Title,
                Content = request.Content,
                EditedById = request.EditedByProfileId,
                ContentMark = request.ContentMark,
                IsPublished = request.IsPublished,
                PublishDate = request.PublishDate,
                CreatedAt = new DateTime()
            });

            if(blog != null)
            {
                blogResponse = await GetBlogById(blog.Id);
            }
            
            return blogResponse;
        }

        public async Task<BlogResponse> GetBlogById(int id)
        {
            BlogResponse blogResponse = new BlogResponse();

            const string query = "SELECT * FROM Blogs WHERE Id = @Id";
            var blog = await _db.QueryFirstOrDefaultAsync<Blog>(query, new { Id = id });

            if (blog != null) {

                var EditedByIdProfile = _profileRepository.ViewProfile(blog.EditedById ?? 0);

                blogResponse = new BlogResponse()
                {
                    Id = blog.Id,
                    Title = blog.Title,
                    Content = blog.Content,
                    EditedByProfile = EditedByIdProfile,
                    ContentMark = blog.ContentMark,
                    IsPublished = blog.IsPublished,
                    PublishDate = blog.PublishDate,
                    BlogImg = new MediaImage()
                    {
                        BlobName = blog.BlogImg_BlobName,
                        FileName = blog.BlogImg_FileName,
                        FullUrl = blog.BlogImg_FullUrl
                    },
                    Likes = blog.Likes.Split(',').ToList(),
                };

            }

            return blogResponse;

        }

        public async Task DeleteBlog(int id)
        {
            const string query = "DELETE FROM Blogs WHERE Id = @Id";
            await _db.ExecuteAsync(query, new { Id = id });
        }

        public async Task<BlogResponse> EditBlog(AddEditBlogRequest request)
        {

            BlogResponse blogResponse = new BlogResponse();
            var existingBlog = await GetBlogById(request.Id);
            if (existingBlog == null)
                return blogResponse;

            var sql = @"UPDATE Blogs SET
                  Title = @Title,
                  Content = @Content,
                  EditedBy = @EditedBy,
                  BlogImg_FileName = @FileName,
                  BlogImg_BlobName = @BlobName,
                  BlogImg_FullUrl = @FullUrl,
                  ContentMark = @ContentMark,
                  IsPublished = @IsPublished,
                  PublishDate = @PublishDate
                WHERE Id = @Id";

            await _db.ExecuteAsync(sql, new
            {
                request.Title,
                request.Content,
                request.EditedByProfileId,
                FileName = request.BlogImg?.FileName,
                BlobName = request.BlogImg?.BlobName,
                FullUrl = request.BlogImg?.FullUrl,
                request.ContentMark,
                request.IsPublished,
                request.PublishDate,
                request.Id
            });

            return await GetBlogById(request.Id);
        }

        public async Task UpdateLikes(string blogId, List<string> likes)
        {
            var sql = "UPDATE Blogs SET Likes = @Likes WHERE Id = @Id";
            var likesJson = JsonConvert.SerializeObject(likes);

            await _db.ExecuteAsync(sql, new
            {
                Likes = likesJson,
                Id = blogId
            });
        }


    }
}
