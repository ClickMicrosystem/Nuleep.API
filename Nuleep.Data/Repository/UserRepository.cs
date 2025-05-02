using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Nuleep.Models;
using System.Data;
using Dapper;
using Azure.Core;
using Nuleep.Data.Interface;

namespace Nuleep.Data.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnection _db;

        public UserRepository(IConfiguration config)
        {
            _db = new SqlConnection(config.GetConnectionString("DefaultConnection"));
        }

        public async Task<User> GetUserAsync(string username, string password)
        {
            var sql = "SELECT u.Id, u.Email, u.Password, u.Role, u.ValidateEmail, u.IsProfile, u.CreatedAt, u.IsDelete, s.Customer_Id, s.Period_Start, s.Period_End, s.Status, s.Plan_Id, s.Trial_Start, s.Trial_End, s.Billing_Cycle_Anchor FROM Users u LEFT JOIN Subscriptions s ON u.Id = s.UserId WHERE u.Email = @Email AND u.IsDelete = 0";


            var user = (await _db.QueryAsync<User, Subscription, User>(
                sql,
                (u, s) => { u.subscription = s; return u; },
                new { Email =  username },
                splitOn: "Customer_Id"
            )).FirstOrDefault();

            return user!;
        }
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            var sql = "SELECT * FROM Users WHERE Email = @Email";
            return await _db.QueryFirstOrDefaultAsync<User>(sql, new { Email = username });
        }
    }
}
