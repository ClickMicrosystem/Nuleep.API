using Nuleep.Models;
using Nuleep.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Business.Interface
{
    public interface IChatService
    {
        Task<(IEnumerable<Message> Messages, int Total)> GetAllMessages(int roomId, int limit, int page);
        Task<Message> AddMessage(string message, int userId, int roomId);
    }
}
