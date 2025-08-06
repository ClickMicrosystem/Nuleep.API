using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Nuleep.Models;
using System.Data;
using Dapper;
using Azure.Core;
using Nuleep.Data.Interface;
using Nuleep.Models.Request;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Text;
using Nuleep.Models.Response;

namespace Nuleep.Data.Repository
{
    public class EventRepository : IEventRepository
    {
        private readonly IDbConnection _db;

        public EventRepository(IConfiguration config)
        {
            _db = new SqlConnection(config.GetConnectionString("DefaultConnection"));
        }

        public async Task<Events> AddEvent(EventEditCreateRequest request)
        {
            var sql = @"INSERT INTO Events 
                   (Title, Summary, Description, StartDate, EndDate, Price, TeleconferenceLink, EventPhysicalAddress, Location, EventImgFileName, EventImgBlobName, EventImgFullUrl, CreatedAt, UpdatedAt, Tags) 
                   VALUES 
                   (@Title, @Summary, @Description, @StartDate, @EndDate, @Price, @TeleconferenceLink, @EventPhysicalAddress, @Location, @EventImgFileName, @EventImgBlobName, @EventImgFullUrl, @CreatedAt, @UpdatedAt, @Tags);
                   SELECT * from Events Where Id = CAST(SCOPE_IDENTITY() as int);";

            return await _db.ExecuteScalarAsync<Events>(sql, new {
                Title = request.Title,
                Summary = request.Summary,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Price = request.Price,
                TeleconferenceLink = request.TeleconferenceLink,
                EventPhysicalAddress = request.EventPhysicalAddress,
                Location = request.Location,
                EventImgFileName = request.MediaImage?.FileName,
                EventImgBlobName = request.MediaImage?.BlobName,
                EventImgFullUrl = request.MediaImage?.FullUrl,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Tags = string.Join(",", request.EventTags)
            });
        }

        public async Task<List<Events>> GetEvents(bool? isDelete, int limit, int offset)
        {
            var sql = @"SELECT * FROM Events 
                        WHERE (@IsDelete IS NULL OR IsDelete = @IsDelete)
                        ORDER BY Id DESC
                        OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY;";

            return (await _db.QueryAsync<Events>(sql, new
            {
                IsDelete = isDelete,
                Offset = offset,
                Limit = limit
            })).ToList();
        }

        public async Task<int> GetEventsCount(bool? isDelete)
        {
            var sql = @"SELECT COUNT(*) FROM Events 
                WHERE (@IsDelete IS NULL OR IsDelete = @IsDelete);";

            return await _db.ExecuteScalarAsync<int>(sql, new { IsDelete = isDelete });
        }

        public async Task<List<Events>> GetJobSeekerEvents(JobSeekerEventFilterRequest request)
        {
            int offset = (request.Page - 1) * request.Limit;
            var now = DateTime.UtcNow;

            var sql = new StringBuilder(@"
                        SELECT * FROM Events
                        WHERE IsDelete = 0
                        AND EndDate >= @Now
                    ");

            var parameters = new DynamicParameters();
            parameters.Add("Now", now);

            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                sql.Append(" AND Title LIKE @Title");
                parameters.Add("Title", $"%{request.Title}%");
            }

            if (!string.IsNullOrWhiteSpace(request.PostDate))
            {
                if (DateTime.TryParse(request.PostDate, out var postDate))
                {
                    parameters.Add("PostDateStart", postDate.Date);
                    parameters.Add("PostDateEnd", postDate.Date.AddDays(1).AddTicks(-1));

                    sql.Append(" AND CreatedAt >= @PostDateStart AND CreatedAt <= @PostDateEnd");
                }
            }

            if (request.EventTags != null && request.EventTags.Count > 0)
            {
                sql.Append(" AND (");

                for (int i = 0; i < request.EventTags.Count; i++)
                {
                    var tagParam = $"@Tag{i}";
                    sql.Append($" EventTags LIKE {tagParam} ");
                    parameters.Add(tagParam, $"%{request.EventTags[i]}%");

                    if (i < request.EventTags.Count - 1)
                        sql.Append(" OR ");
                }

                sql.Append(")");
            }

            sql.Append(" ORDER BY Id DESC OFFSET @Offset ROWS FETCH NEXT @Limit ROWS ONLY;");
            parameters.Add("Offset", offset);
            parameters.Add("Limit", request.Limit);

            var result = await _db.QueryAsync<Events>(sql.ToString(), parameters);
            return result.ToList();
        }

        public async Task<int> GetJobSeekerEventsCount(JobSeekerEventFilterRequest request)
        {
            var now = DateTime.UtcNow;

            var sql = new StringBuilder(@"
                        SELECT COUNT(*) FROM Events
                        WHERE IsDelete = 0
                        AND EndDate >= @Now
                    ");

            var parameters = new DynamicParameters();
            parameters.Add("Now", now);

            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                sql.Append(" AND Title LIKE @Title");
                parameters.Add("Title", $"%{request.Title}%");
            }

            if (!string.IsNullOrWhiteSpace(request.PostDate))
            {
                if (DateTime.TryParse(request.PostDate, out var postDate))
                {
                    parameters.Add("PostDateStart", postDate.Date);
                    parameters.Add("PostDateEnd", postDate.Date.AddDays(1).AddTicks(-1));

                    sql.Append(" AND CreatedAt >= @PostDateStart AND CreatedAt <= @PostDateEnd");
                }
            }

            if (request.EventTags != null && request.EventTags.Count > 0)
            {
                sql.Append(" AND (");

                for (int i = 0; i < request.EventTags.Count; i++)
                {
                    var tagParam = $"@Tag{i}";
                    sql.Append($" EventTags LIKE {tagParam} ");
                    parameters.Add(tagParam, $"%{request.EventTags[i]}%");

                    if (i < request.EventTags.Count - 1)
                        sql.Append(" OR ");
                }

                sql.Append(")");
            }

            return await _db.ExecuteScalarAsync<int>(sql.ToString(), parameters);
        }

        public async Task<bool> SoftDeleteEvent(int id)
        {
            var sql = @"UPDATE Events SET IsDelete = 1 WHERE Id = @Id";
            var affectedRows = await _db.ExecuteAsync(sql, new { Id = id });
            return affectedRows > 0;
        }

        public async Task<Events?> GetEventById(int id)
        {
            var sql = "SELECT TOP 1 * FROM Events WHERE Id = @Id";
            return await _db.QueryFirstOrDefaultAsync<Events>(sql, new { Id = id });
        }

        public async Task<List<RegisteredUser>> GetRegisteredUsers(int eventId)
        {
            var sql = @"
                        SELECT u.Id AS UserId, u.Email, p.FullName, p.JobTitle, p.StreetAddress
                        FROM EventRegistrations er
                        INNER JOIN Users u ON er.UserId = u.Id
                        LEFT JOIN Profile p ON u.Id = p.UserId
                        WHERE er.EventId = @EventId";

            return (await _db.QueryAsync<RegisteredUser>(sql, new { EventId = eventId })).ToList();
        }

        public async Task<List<string>> GetEventTags()
        {
            var sql = @"SELECT EventTags FROM Events 
                        WHERE IsDelete = 0 AND EndDate >= @Now";

            var now = DateTime.UtcNow;
            var rows = await _db.QueryAsync<string>(sql, new { Now = now });

            var tags = new List<string>();

            foreach (var row in rows)
            {
                if (!string.IsNullOrWhiteSpace(row))
                {
                    tags.AddRange(row.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
                }
            }

            return tags;
        }

        public async Task<bool> EditEvent(EventEditCreateRequest request)
        {
            var sql = new StringBuilder("UPDATE Events SET ");
            var parameters = new DynamicParameters();

            void AddField(string fieldName, object? value)
            {
                if (value != null)
                {
                    sql.Append($"{fieldName} = @{fieldName}, ");
                    parameters.Add(fieldName, value);
                }
            }

            AddField("Title", request.Title);
            AddField("Summary", request.Summary);
            AddField("Description", request.Description);
            AddField("TeleconferenceLink", request.TeleconferenceLink);
            AddField("EventPhysicalAddress", request.EventPhysicalAddress);
            AddField("Location", request.Location);
            AddField("EventTags", request.EventTags);
            AddField("StartDate", request.StartDate);
            AddField("EndDate", request.EndDate);
            AddField("EventImgFileName", request.MediaImage?.FileName);
            AddField("EventImgBlobName", request.MediaImage?.BlobName);
            AddField("EventImgFullUrl", request.MediaImage?.FullUrl);
            AddField("UpdatedAt", DateTime.Now);
            AddField("Tags", string.Join(",", request.EventTags));

            if (!string.IsNullOrEmpty(request.Price))
            {
                if (decimal.TryParse(request.Price.Replace(",", ""), out var parsedPrice))
                {
                    AddField("Price", parsedPrice);
                }
            }

            // Remove trailing comma
            if (parameters.ParameterNames.Any())
            {
                sql.Length -= 2;
                sql.Append(" WHERE Id = @Id");
                parameters.Add("Id", request.Id);

                var affectedRows = await _db.ExecuteAsync(sql.ToString(), parameters);
                return affectedRows > 0;
            }

            return false;
        }

        public async Task<bool> RegisterUserForEvent(int eventId, int userId)
        {
            var eventExists = await _db.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM Events WHERE Id = @EventId AND IsDelete = 0",
                new { EventId = eventId });

            if (eventExists == 0) return false;

            var alreadyRegistered = await _db.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM RegisteredUsers WHERE EventId = @EventId AND UserId = @UserId",
                new { EventId = eventId, UserId = userId });

            if (alreadyRegistered > 0) return true;

            var insertSql = @"
                                INSERT INTO RegisteredUsers (EventId, UserId)
                                VALUES (@EventId, @UserId)";

            var rows = await _db.ExecuteAsync(insertSql, new { EventId = eventId, UserId = userId });
            return rows > 0;
        }

        public async Task<bool> UnregisterUserFromEvent(int eventId, int userId)
        {
            // Check if the event exists
            var eventExists = await _db.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM Events WHERE Id = @EventId AND IsDelete = 0",
                new { EventId = eventId });

            if (eventExists == 0)
                return false;

            // Update the registration
            var sql = @"UPDATE RegisteredUsers 
                SET IsRefunded = 1 
                WHERE EventId = @EventId AND UserId = @UserId";

            var rows = await _db.ExecuteAsync(sql, new { EventId = eventId, UserId = userId });
            return rows > 0;
        }


    }
}
