using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Nuleep.Models;
using System.Data;
using Dapper;
using Azure.Core;
using Nuleep.Data.Interface;
using Nuleep.Models.Request;

namespace Nuleep.Data.Repository
{
    public class NotificationsRepository : INotificationsRepository
    {
        private readonly IDbConnection _db;

        public NotificationsRepository(IConfiguration config)
        {
            _db = new SqlConnection(config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<Notification>> GetNotifications(GetNotificationsRequest request)
        {

            var sql = @"SELECT * FROM Notifications
                        WHERE UserId = @UserId
                          AND NotificationType IN @NotificationType
                        ORDER BY Id DESC";

            return await _db.QueryAsync<Notification>(sql, new
            {
                request.UserId,
                request.NotificationType
            });
        }

        public async Task<Notification> AddNotification(Notification notification)
        {
            var sql = @"
            INSERT INTO Notifications (Title, UserId, RoomId, NotificationType, IsRead, CreatedAt, UpdatedAt)
            OUTPUT INSERTED.*
            VALUES (@Title, @UserId, @RoomId, @NotificationType, @IsRead, GETDATE(), GETDATE())";

            return await _db.QueryFirstOrDefaultAsync<Notification>(sql, notification);
        }

        public async Task<int> MarkAsRead(ReadNotificationRequest request)
        {
            if (request.Id.HasValue)
            {
                return await _db.ExecuteAsync(
                    "UPDATE Notifications SET IsRead = 1 WHERE Id = @Id",
                    new { request.Id });
            }

            var conditions = new List<string>();
            var parameters = new DynamicParameters();

            if (!string.IsNullOrEmpty(request.UserId))
            {
                conditions.Add("UserId = @UserId");
                parameters.Add("UserId", request.UserId);
            }

            if (request.NotificationType?.Any() == true)
            {
                conditions.Add("NotificationType IN @Types");
                parameters.Add("Types", request.NotificationType);
            }

            if (!string.IsNullOrEmpty(request.RoomId))
            {
                conditions.Add("RoomId = @RoomId");
                parameters.Add("RoomId", request.RoomId);
            }

            var whereClause = conditions.Any() ? "WHERE " + string.Join(" AND ", conditions) : "";

            var sql = $"UPDATE Notifications SET IsRead = 1 {whereClause}";

            return await _db.ExecuteAsync(sql, parameters);
        }
    }
}
