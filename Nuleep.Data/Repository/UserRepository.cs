using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Nuleep.Models;
using System.Data;
using Dapper;
using Azure.Core;
using Nuleep.Data.Interface;
using DocumentFormat.OpenXml.Spreadsheet;
using Nuleep.Models.Request;

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
        public async Task<User> GetUserByUsername(string username)
        {
            var sql = "SELECT * FROM Users WHERE Email = @Email";
            return await _db.QueryFirstOrDefaultAsync<User>(sql, new { Email = username });
        }

        public async Task<int> CreateUser(User user)
        {
            var sql = @"INSERT INTO Users (Email, Password, Role, GoogleId)
                VALUES (@Email, @Password, @Role, @GoogleId);
                SELECT CAST(SCOPE_IDENTITY() as int)";
            return await _db.ExecuteScalarAsync<int>(sql, new
            {
                Email = user.Email,
                Password = user.Password,
                Role = user.Role,
                GoogleId = user.GoogleId
            });
        }

        public async Task UpdateResetToken(int userId, string hashedToken, DateTime expiry)
        {
            var sql = @"UPDATE Users 
                SET ResetPasswordToken = @Token, ResetPasswordExpire = @Expire 
                WHERE Id = @UserId";
            await _db.ExecuteAsync(sql, new { Token = hashedToken, Expire = expiry, UserId = userId });
        }

        public async Task ClearResetToken(int userId)
        {
            var sql = @"UPDATE Users 
                SET ResetPasswordToken = NULL, ResetPasswordExpire = NULL 
                WHERE Id = @UserId";
            await _db.ExecuteAsync(sql, new { UserId = userId });
        }

        public async Task<User?> GetUserByResetToken(string hashedToken)
        {
            var sql = @"SELECT * FROM Users 
                WHERE ResetPasswordToken = @Token 
                AND ResetPasswordExpire > @Now";

            return await _db.QueryFirstOrDefaultAsync<User>(sql, new
            {
                Token = hashedToken,
                Now = DateTime.UtcNow
            });
        }

        public async Task UpdatePasswordAndClearToken(int userId, string newPasswordHash)
        {
            var sql = @"UPDATE Users
                SET Password = @Password, 
                    ResetPasswordToken = NULL, 
                    ResetPasswordExpire = NULL
                WHERE Id = @UserId";

            await _db.ExecuteAsync(sql, new
            {
                Password = newPasswordHash,
                UserId = userId
            });
        }

        public async Task<User?> UpdateIsProfileStatus(int id, bool isProfile)
        {
            var sql = "UPDATE Users SET IsProfile = @IsProfile WHERE Id = @Id";

            await _db.ExecuteAsync(sql, new { Id = id, IsProfile = isProfile });

            // Return updated user
            return await _db.QueryFirstOrDefaultAsync<User>("SELECT * FROM Users WHERE Id = @Id", new { Id = id });
        }

        public async Task<User?> UpdateEmailVerifiedStatus(int id, bool isEmailVerified)
        {
            var sql = "UPDATE Users SET ValidateEmail = @ValidateEmail WHERE Id = @Id";

            await _db.ExecuteAsync(sql, new { Id = id, ValidateEmail = isEmailVerified });

            // Return updated user
            return await _db.QueryFirstOrDefaultAsync<User>("SELECT * FROM Users WHERE Id = @Id", new { Id = id });
        }

        public async Task RemoveEmployee(RemoveEmployeeRequest request)
        {
            string deleteUser = "UPDATE Users SET IsDelete = 1 WHERE Id = @Id";
            await _db.ExecuteAsync(deleteUser, new { Id = request.UId });

            string deleteProfiles = "UPDATE Profile SET IsDelete = 1 WHERE UserRef = @Id";
            await _db.ExecuteAsync(deleteProfiles, new { Id = request.UId });
        }
        public async Task UpdateGoogleId(int userId, string googleId)
        {
            var sql = @"UPDATE Users 
                SET GoogleId = @GoogleId
                WHERE Id = @UserId";
            await _db.ExecuteAsync(sql, new { GoogleId = googleId,  UserId = userId });
        }


    }
}
