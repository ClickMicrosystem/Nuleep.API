using Microsoft.AspNetCore.Mvc;
using Nuleep.Business.Interface;
using Nuleep.Models.Request;

namespace Nuleep.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("getMessages")]
        public async Task<IActionResult> GetAllMessages([FromBody] GetMessagesRequest request)
        {
            var (messages, total) = await _chatService.GetAllMessages(request.RoomId, request.Limit ?? 10, (request.Page ?? 1) - 1);
            return Ok(new { success = true, data = new { data = messages, total } });
        }

        [HttpPost("addMessages")]
        public async Task<IActionResult> AddMessage([FromBody] AddMessageRequest request)
        {
            var result = await _chatService.AddMessage(request.Message, request.UserId, request.RoomId);
            return Ok(new { success = true, data = result });
        }
    }
}
