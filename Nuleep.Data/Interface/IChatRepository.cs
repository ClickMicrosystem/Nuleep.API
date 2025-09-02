using Nuleep.Models;
using Nuleep.Models.Request;
using Nuleep.Models.Response;

namespace Nuleep.Data.Interface
{
    public interface IChatRepository
    {
        Task<IEnumerable<Message>> GetAllMessages(int roomId, int limit, int page);
        Task<int> GetTotalMessagesCount(int roomId);
        Task<Message> AddMessage(Message message);


    }
}
