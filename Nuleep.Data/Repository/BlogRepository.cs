using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Nuleep.Models;
using System.Data;
using Dapper;
using Nuleep.Models.Blogs;
using Azure.Core;
using Nuleep.Data.Interface;

namespace Nuleep.Data.Repository
{
    public class BlogRepository : IBlogRepository
    {
        private readonly IDbConnection _db;

        public BlogRepository(IConfiguration config)
        {
            _db = new SqlConnection(config.GetConnectionString("DefaultConnection"));
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
    }
}
