using Azure.Core;
using Nuleep.Business.Interface;
using Nuleep.Data.Interface;
using Nuleep.Models;
using Nuleep.Models.Request;
using Nuleep.Models.Response;

namespace Nuleep.Business.Services
{
    public class ChatService : IChatService
    {
        
        private readonly IChatRepository _chatRepository;

        public ChatService(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        public async Task<(IEnumerable<Message> Messages, int Total)> GetAllMessages(int roomId, int limit, int page)
        {
            var messages = await _chatRepository.GetAllMessages(roomId, limit, page);
            var total = await _chatRepository.GetTotalMessagesCount(roomId);
            return (messages, total);
        }

        public async Task<Message> AddMessage(string message, int userId, int roomId)
        {
            var chatMessage = new Message
            {
                MessageContent = message,
                EditedBy = userId,
                RoomId = roomId,
            };

            return await _chatRepository.AddMessage(chatMessage);
        }

    }
}
