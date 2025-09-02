using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Nuleep.Models;
using System.Data;
using Dapper;
using Azure.Core;
using Nuleep.Data.Interface;
using Nuleep.Models.Request;
using Nuleep.Models.Response;
using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Reflection;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;

namespace Nuleep.Data.Repository
{
    public class ChatRepository : IChatRepository
    {
        private readonly IDbConnection _db;

        public ChatRepository(IConfiguration config)
        {
            _db = new SqlConnection(config.GetConnectionString("DefaultConnection"));
        }

        public async Task<IEnumerable<Message>> GetAllMessages(int roomId, int limit, int page)
        {
            var sql = @"SELECT * FROM Chats
                    WHERE RoomId = @RoomId
                    ORDER BY Id DESC
                    OFFSET @Offset ROWS
                    FETCH NEXT @Limit ROWS ONLY;";

            return await _db.QueryAsync<Message>(sql, new
            {
                RoomId = roomId,
                Offset = limit * page,
                Limit = limit
            });
        }

        public async Task<int> GetTotalMessagesCount(int roomId)
        {
            var sql = "SELECT COUNT(*) FROM Chats WHERE RoomId = @RoomId";
            return await _db.ExecuteScalarAsync<int>(sql, new { RoomId = roomId });
        }

        public async Task<Message> AddMessage(Message message)
        {
            var sql = @"INSERT INTO Chats (Message, EditedBy, RoomId, CreatedAt)
                    VALUES (@Message, @EditedBy, @RoomId, Getdate());
                    SELECT CAST(SCOPE_IDENTITY() as int);";

            var id = await _db.ExecuteScalarAsync<int>(sql, new
            {
                Message = message.MessageContent,
                EditedBy = message.EditedBy,
                RoomId = message.RoomId
            });
            message.Id = id;
            return message;
        }
    }
}
